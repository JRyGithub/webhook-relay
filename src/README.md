# Backend projects

What each .NET project is for, what it depends on, and what's in its `.csproj`. Shared build settings (`net10.0`, nullable, implicit usings, warnings as errors, analyzers) come from [Directory.Build.props](../Directory.Build.props) at the repo root, so the individual `.csproj` files don't repeat them.

## How they fit together

```
AppHost  ──orchestrates──▶  Api, Worker, FlakyTarget, web, Postgres
                               │      │         │
                               ├──────┴─────────┴──▶ ServiceDefaults (shared setup)
                               └──────┬──▶ Data (DbContext + entities)
                                      │
Tests ────────────────────────────────┘
```

## WebhookRelay.AppHost: the orchestrator

- **SDK:** `Aspire.AppHost.Sdk`, which turns this into an Aspire host. Its project references to Api, Worker and FlakyTarget generate the `Projects.WebhookRelay_*` classes used in [AppHost.cs](WebhookRelay.AppHost/AppHost.cs).
- **Packages:** `Aspire.Hosting.PostgreSQL` provides `AddPostgres`, and `Aspire.Hosting.JavaScript` provides `AddViteApp`.
- **`UserSecretsId`:** where Aspire stores the generated Postgres password.
- **Role:** starts Postgres in Docker plus every service, in dependency order (`WaitFor`), and injects connection strings and service URLs (`WithReference`). It runs only during development and contains no business logic.

## WebhookRelay.ServiceDefaults: shared setup

- **Kind:** a class library that every service calls with one line, `builder.AddServiceDefaults()`.
- **Provides:**
  - OpenTelemetry tracing, metrics and logs, exported to the Aspire dashboard
  - health checks (`/health`, `/alive`)
  - service discovery, so `http://flakytarget` resolves
  - HttpClient defaults
- **Watch out:** it adds `AddStandardResilienceHandler()` (automatic retries) to **every** `HttpClient`. The delivery client must opt out so it doesn't mask the relay's own retry logic.

## WebhookRelay.Data: the persistence layer

- **Kind:** a class library with `Npgsql.EntityFrameworkCore.PostgreSQL` (EF Core plus the Postgres provider).
- **Holds:** `RelayDbContext`, plus entities, `IEntityTypeConfiguration` classes and migrations as they're added.
- **Why separate:** the Api, Worker and Tests share one schema definition.

## WebhookRelay.Api: the HTTP front door

- **SDK:** `Microsoft.NET.Sdk.Web`, an ASP.NET Core minimal API.
- **Packages:**
  - `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` provides `AddNpgsqlDbContext`, which reads the injected connection string and adds DB health checks, retries and tracing.
  - `Microsoft.AspNetCore.OpenApi` generates the OpenAPI document.
  - `Microsoft.EntityFrameworkCore.Design` is needed by `dotnet ef`, because this is the migrations startup project. It's marked `PrivateAssets` so it doesn't ship.
- **Role:** receives webhooks at `POST /hooks/{source}`, stores them, then acknowledges the sender. It also serves the dashboard API.

## WebhookRelay.Worker: the delivery engine

- **SDK:** `Microsoft.NET.Sdk.Worker`, a long-running background process with no HTTP server.
- **Role:** `DeliveryWorker` (a `BackgroundService`) claims pending deliveries, forwards them to targets, records each attempt, retries with backoff and dead-letters after N failures.
- **Why separate:** it can be scaled (`WithReplicas(2)`) and killed independently. That's the point of the concurrency and failure-injection milestones.

## WebhookRelay.FlakyTarget: the test receiver

- **SDK:** `Microsoft.NET.Sdk.Web`, a tiny web app.
- **Role:** stands in for "the app receiving webhooks", and can be switched to succeed, return 503 or respond slowly. This gives the relay something realistic to retry against.
- **Not part of the product:** it's development and testing tooling.

## WebhookRelay.Tests (in `../tests/`)

- **Packages:** xUnit, the test SDK and coverlet (coverage). It references Data.
- **Role:** unit tests for retry, backoff and signature logic, and integration tests against real Postgres via Testcontainers.
