# 2. ASP.NET Core minimal APIs instead of controllers

Status: Accepted

## Context
The API surface is small: one ingestion route (`POST /hooks/{source}`) and a handful of dashboard routes. The ingestion route must read the raw request body (for HMAC verification) and respond fast.

## Decision
Use minimal APIs, grouped with `MapGroup`, with handlers as static methods or small classes when they grow.

## Alternatives considered
- **MVC controllers.** These add model binding and filters we'd partly fight. Reading the raw body is more awkward when the input formatters have already consumed the stream.

## Consequences
- Less ceremony, and the request pipeline is explicit and easy to trace.
- Cross-cutting concerns (auth, rate limiting, validation) are attached per group with endpoint filters and metadata, not attributes.
- Organisation is up to us: once `Program.cs` grows, move route groups into `Endpoints/*.cs` extension methods.
