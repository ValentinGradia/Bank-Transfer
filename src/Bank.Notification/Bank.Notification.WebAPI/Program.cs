using Bank.Notification.WebAPI.Application.Database;
using Bank.Notification.WebAPI.Application.Features.Process;
using Bank.Notification.WebAPI.Application.Handlers;
using Bank.Notification.WebAPI.External.ServiceBusReceive;
using Bank.Notification.WebAPI.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IProcessService, ProcessService>();

builder.Services.AddHostedService<ServiceBusReceiveService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly));

var app = builder.Build();

app.Run();