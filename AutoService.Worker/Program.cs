using AutoService.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddHostedService<NotificationWorker>();

await builder.Build().RunAsync();
