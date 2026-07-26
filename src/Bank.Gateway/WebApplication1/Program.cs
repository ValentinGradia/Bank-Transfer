using WebApplication1.Application.External;
using WebApplication1.Application.Features;
using WebApplication1.Application.Models;
using WebApplication1.External;

var builder = WebApplication.CreateBuilder(args);

// Designed to be shared across the application, this service is registered as a singleton to ensure a single instance is used throughout the application's lifetime.
builder.Services.AddSingleton<IServiceBusSenderService, ServiceBusSenderService>();

builder.Services.AddScoped<IProcessService, ProcessService>();

var app = builder.Build();
ApiGatewayEndpoint.GatewayEndpoint(app);
app.Run();
