# Webhook Relay

A relay that sits between a service that sends webhooks (Stripe, GitHub) and the app that receives them, and guarantees delivery when things go wrong. It works like this:

1. It accepts the webhook and stores it in Postgres.
2. It acknowledges the sender.
3. It forwards the webhook to one or more targets with retries and exponential backoff.
4. It dead-letters the webhook after repeated failures.
5. You can replay any event from a React dashboard.

This is a learning project, built milestone by milestone. Architecture decisions live in [adr/](adr/).

## Prerequisites

| Tool | Version | Check |
|------|---------|-------|
| .NET SDK | 10.0 | `dotnet --list-sdks` |
| Docker | any recent (Docker Desktop with WSL integration on Windows) | `docker version` |
| Node.js | 24 | `node --version` |
| pnpm | 11 | `pnpm --version` |

Aspire is pulled in through NuGet, so it doesn't need a separate install. The Aspire project templates (`dotnet new install Aspire.ProjectTemplates`) are only needed to generate new projects.

## Run everything

```sh
cd web && pnpm install && cd ..
dotnet run --project src/WebhookRelay.AppHost --launch-profile http
```

The console prints a link to the **Aspire dashboard** (`http://localhost:15030/login?t=…`). Open it to see every resource, its URL, logs, traces and metrics.

Aspire starts these resources:

| Resource | What it is |
|----------|------------|
| `postgres` / `relaydb` | Postgres in Docker. Data persists in a named volume across runs. |
| `api` | Minimal API on <http://localhost:5062>. `GET /api/status` checks database connectivity. |
| `worker` | Background service that delivers stored events. |
| `flakytarget` | Test receiver for simulating failing targets. |
| `web` | Vite + React dashboard. Its URL is on the Aspire dashboard, and it proxies `/api` to the API. |

Stop everything with `Ctrl+C`.

> The `http` profile is used so you don't have to trust HTTPS dev certificates (awkward under WSL). To use HTTPS instead, run `dotnet dev-certs https --trust` and pass `--launch-profile https`.

## Build, lint and test

These are the same checks CI runs ([.github/workflows/ci.yml](.github/workflows/ci.yml)).

```sh
# .NET: warnings are errors, analyzers enabled (see Directory.Build.props)
dotnet build
dotnet test

# Web
cd web
pnpm typecheck
pnpm lint      # oxlint
pnpm build
```

## Database migrations

The EF Core `DbContext` lives in `src/WebhookRelay.Data`. When you add entities:

```sh
dotnet tool install --global dotnet-ef   # once
dotnet ef migrations add <Name> --project src/WebhookRelay.Data --startup-project src/WebhookRelay.Api
```

## Project layout

See [src/README.md](src/README.md) for what each .NET project does.

```
src/
  WebhookRelay.AppHost/          Aspire orchestration: wires up all resources
  WebhookRelay.ServiceDefaults/  OpenTelemetry, health checks, HttpClient defaults
  WebhookRelay.Data/             EF Core DbContext, entities, migrations
  WebhookRelay.Api/              Webhook ingestion and dashboard API
  WebhookRelay.Worker/           Delivery loop (retries, backoff, dead-lettering)
  WebhookRelay.FlakyTarget/      Configurable receiver for failure testing
web/                             React dashboard (strict TS, TanStack Query, Zod)
tests/WebhookRelay.Tests/        xUnit tests
adr/                             Architecture decision records and AI review log
```

## Troubleshooting

- **`dotnet` crashes with `lstat("/run/user/1000/")` … `ENOENT` (WSL).** The per-user runtime folder is missing. Run `wsl --shutdown` from Windows and reopen the terminal, or run `sudo loginctl enable-linger $USER`.
- **AppHost fails with "applicationUrl must be an https address".** You're using a profile without `ASPIRE_ALLOW_UNSECURED_TRANSPORT`. Pass `--launch-profile http`.
- **Postgres won't start.** Check that Docker is running and reachable from your shell (`docker ps`).
