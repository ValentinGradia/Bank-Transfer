using WebApplication1.Application.External;
using WebApplication1.Application.Models;
using WebApplication1.External;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IServiceBusSenderService, ServiceBusSenderService>();

var app = builder.Build();
ApiGatewayEndpoint.GatewayEndpoint(app);
app.Run();
