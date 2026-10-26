var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var db = postgres.AddDatabase("relaydb");

var flakyTarget = builder.AddProject<Projects.WebhookRelay_FlakyTarget>("flakytarget");

var api = builder.AddProject<Projects.WebhookRelay_Api>("api")
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<Projects.WebhookRelay_Worker>("worker")
    .WithReference(db)
    .WithReference(flakyTarget)
    .WaitFor(db);

builder.AddViteApp("web", "../../web")
    .WithPnpm()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
