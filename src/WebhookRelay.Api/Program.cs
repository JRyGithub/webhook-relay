using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebhookRelay.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<RelayDbContext>("relaydb");
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();

// M1: POST /hooks/{source} goes here.

app.MapGet("/api/status", async (RelayDbContext db, CancellationToken ct) =>
    new StatusResponse(await db.Database.CanConnectAsync(ct)));

app.Run();

internal sealed record StatusResponse(bool DatabaseReachable);
