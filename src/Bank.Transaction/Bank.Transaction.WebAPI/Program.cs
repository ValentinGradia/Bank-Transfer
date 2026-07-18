using Bank.Transaction.WebAPI.Application.Database;
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

var app = builder.Build();

app.MapGet("/transaction", async ([FromServices] IDatabaseService databaseService) =>
{
    var data = await databaseService.Transaction.ToListAsync();
    return data;
});
 
app.Run();
