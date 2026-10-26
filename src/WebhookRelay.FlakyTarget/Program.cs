var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// M1: a receive endpoint whose behaviour can be switched (succeed, 503, sleep N seconds).

app.Run();
