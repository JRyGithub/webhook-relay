using WebhookRelay.Data;
using WebhookRelay.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<RelayDbContext>("relaydb");
builder.Services.AddHostedService<DeliveryWorker>();

var host = builder.Build();
host.Run();
