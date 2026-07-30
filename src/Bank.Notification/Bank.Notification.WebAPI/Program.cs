using Bank.Notification.WebAPI.Application.Database;
using Bank.Notification.WebAPI.Domain.Entities;
using Bank.Notification.WebAPI.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDatabaseService, DatabaseService>();

var app = builder.Build();

app.Run();