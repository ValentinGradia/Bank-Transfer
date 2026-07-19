using Bank.Transfer.WebAPI.Application.Database;
using Bank.Transfer.WebAPI.Persistence.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<DatabaseService>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<IDatabaseService, DatabaseService>();


var app = builder.Build();

app.MapGet("/transfer", async ([FromServices] IDatabaseService databaseService) =>
{
    var data = await databaseService.Transfer.ToListAsync();
    return data;
});


app.Run();
