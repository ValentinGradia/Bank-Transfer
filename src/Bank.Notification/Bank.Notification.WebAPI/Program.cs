using Bank.Notification.WebAPI.Application.Database;
using Bank.Notification.WebAPI.Domain.Entities;
using Bank.Notification.WebAPI.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDatabaseService, DatabaseService>();

var app = builder.Build();

app.MapGet("/notification", async ([FromServices] IDatabaseService databaseService) =>
{
    var entity = new NotificationEntity
    {
        CorrelationId = Guid.NewGuid().ToString(),
        CustomerId = 1,
        Type = "Email",
        Content = "This is a test notification.",
        TransactionStatus = true
    };
    
    await databaseService.AddNotificationAsync(entity);
    var notifications = await databaseService.GetAllNotificationsAsync();
    return notifications;
});

app.Run();