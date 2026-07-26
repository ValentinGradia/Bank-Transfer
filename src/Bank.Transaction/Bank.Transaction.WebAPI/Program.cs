using Bank.Transaction.WebAPI.Application.Database;
using Bank.Transaction.WebAPI.Application.Features.Process;
using Bank.Transaction.WebAPI.Application.Handlers;
using Bank.Transaction.WebAPI.External.ServiceBusReceive;
using Bank.Transaction.WebAPI.Persistence.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DatabaseService>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("BankTransactionConnection")));
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IProcessService, ProcessService>();

builder.Services.AddHostedService<ServiceBusReceiveService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly));

var app = builder.Build();

app.MapGet("/transaction", async ([FromServices] IDatabaseService databaseService) =>
{
    var data = await databaseService.Transaction.ToListAsync();
    return data;
});
 
app.Run();
