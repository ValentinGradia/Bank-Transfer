using Bank.Balance.WebAPI.Application.Database;
using Bank.Balance.WebAPI.Peristence.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<DatabaseService>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

var app = builder.Build();

app.MapGet("/balance", async ([FromServices] IDatabaseService databaseService) =>
{
    var data = await databaseService.Balance.ToListAsync();
    return data;
});

app.Run();
