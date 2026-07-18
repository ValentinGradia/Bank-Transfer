using WebApplication1.Application.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
ApiGatewayEndpoint.GatewayEndpoint(app);
app.Run();
