using Bank.Balance.WebAPI.Application.Database;
using Bank.Balance.WebAPI.Application.External;
using Bank.Balance.WebAPI.Application.Features.Process;
using Bank.Balance.WebAPI.Application.Handlers;
using Bank.Balance.WebAPI.External.ServiceBusReceive;
using Bank.Balance.WebAPI.External.ServiceBusSender;
using Bank.Balance.WebAPI.Persistence.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseService>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddSingleton<IServiceBusSenderService, ServiceBusSenderService>();

builder.Services.AddHostedService<ServiceBusReceiveService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly));

var app = builder.Build();

app.Run();
