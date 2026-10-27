using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<BehaviorStore>();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddLogging();

var app = builder.Build();

app.MapDefaultEndpoints();


app.MapPost("/receive", async (HttpRequest request, BehaviorStore store, ILogger<Program> logger, CancellationToken ct) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync(ct);

    var behavior = store.Get();
    logger.WebhookReceived(body.Length, behavior.Mode);

    if (behavior.Mode == TargetMode.Sleep)
    {
        await Task.Delay(behavior.Delay, ct);
    }
    return behavior.Mode switch
    {
        TargetMode.Succeed or TargetMode.Sleep => Results.Ok(),
        TargetMode.Fail => Results.StatusCode(StatusCodes.Status503ServiceUnavailable),
        _ => throw new UnreachableException($"Unhandled mode {behavior.Mode}"),
    };
});

app.MapGet("/mode", (BehaviorStore store) =>
{
    var behavior = store.Get();
    return Results.Ok(behavior);
});

app.MapPost("/mode", (BehaviorStore store, [FromQuery(Name = "mode")] TargetMode mode, [FromQuery(Name = "delay_ms")] int delayMs = 0) =>
{
    if (!Enum.IsDefined(mode))
    {
        return Results.BadRequest($"Unknown mode '{mode}'.");
    }

    if (delayMs < 0)
    {
        return Results.BadRequest($"Invalid delay_ms '{delayMs}'.");
    }

    store.Set(new TargetBehavior(mode, TimeSpan.FromMilliseconds(delayMs)));
    return Results.Ok(store.Get());
});

app.Run();

internal enum TargetMode
{
    Succeed,
    Fail,
    Sleep
}

internal sealed record TargetBehavior(TargetMode Mode, TimeSpan Delay);

internal sealed class BehaviorStore
{
    private volatile TargetBehavior _current = new(TargetMode.Succeed, TimeSpan.Zero);

    public TargetBehavior Get() => _current;

    public void Set(TargetBehavior behavior) => _current = behavior;
}

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information,
        Message = "Received webhook ({BodyLength} bytes), responding as {Mode}")]
    public static partial void WebhookReceived(this ILogger logger, int bodyLength, TargetMode mode);
}