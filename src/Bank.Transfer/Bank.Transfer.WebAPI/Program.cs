using Bank.Transfer.WebAPI.Application.Database;
using Bank.Transfer.WebAPI.Application.External;
using Bank.Transfer.WebAPI.Application.Features.Process;
using Bank.Transfer.WebAPI.Application.Handlers;
using Bank.Transfer.WebAPI.External.ServiceBusReceive;
using Bank.Transfer.WebAPI.External.ServiceBusSender;
using Bank.Transfer.WebAPI.Persistence.Database;
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
