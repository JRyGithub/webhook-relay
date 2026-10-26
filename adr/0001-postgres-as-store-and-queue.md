# 1. Postgres as both event store and delivery queue

Status: Accepted (draft — revisit after M3)

## Context
The relay must persist every incoming webhook before acknowledging it, and then deliver it to one or more targets with retries. That needs durable storage and a work queue with at-least-once semantics.

## Decision
Use a single Postgres database for both. Delivery jobs are rows in a `deliveries` table. Workers claim them with `SELECT … FOR UPDATE SKIP LOCKED` and hold a time-limited lease (`locked_until`), so a crashed worker's jobs become claimable again.

## Alternatives considered
- **RabbitMQ / SQS / Azure Service Bus.** These give purpose-built queue semantics, but add a second system to operate. Storing the event and enqueueing it can't happen in one transaction, so we'd need an outbox anyway.
- **Hangfire / Quartz.** These hide the locking and retry mechanics this project exists to learn.

## Consequences
- Storing an event and creating its delivery rows happens in one transaction, so no outbox is needed.
- Polling adds latency (bounded by the poll interval). `LISTEN/NOTIFY` can reduce it later.
- Throughput is capped by Postgres write capacity. That's acceptable for this scale; note it as a limit.
- The claim query and its index become the hot path, and are reviewed in M9.
