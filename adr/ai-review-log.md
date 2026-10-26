# AI review log

For each agent-generated change, record what you kept, changed or rejected, and why. This feeds the "what AI got right and wrong" write-up at the end.

| Milestone | What the agent did | What I changed / rejected | Why |
|-----------|--------------------|---------------------------|-----|
| M0 | Scaffolded solution, Aspire AppHost, ServiceDefaults, Vite app, CI, ADRs | | |

## Open review points from M0

- `ServiceDefaults` applies `AddStandardResilienceHandler()` to **every** `HttpClient`, which retries automatically. That would silently duplicate or hide the relay's own retry logic (M2). The delivery client needs to opt out, or the default needs to move.
- `MapDefaultEndpoints` only exposes `/health` and `/alive` in Development. Decide what production health checks look like.
- The AppHost `http` launch profile was missing `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true` and crashed on start. I added it (dev only). Run with `--launch-profile http`, or trust the dev certs and use `https`.
- `/api/status` is a scaffold-only endpoint to prove the web → API → DB path. Remove or replace it when the dashboard API lands.
