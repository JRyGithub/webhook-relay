namespace WebhookRelay.Worker;

// M1: poll for stored events and forward them to FlakyTarget.
internal sealed partial class DeliveryWorker(ILogger<DeliveryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Delivery worker started")]
    private static partial void LogStarted(ILogger logger);
}
