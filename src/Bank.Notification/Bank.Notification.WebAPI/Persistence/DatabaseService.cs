using System.Net;
using Bank.Notification.WebAPI.Application.Database;
using Bank.Notification.WebAPI.Domain.Entities;
using Microsoft.Azure.Cosmos;

namespace Bank.Notification.WebAPI.Persistence;

public class DatabaseService : IDatabaseService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;

    public DatabaseService(IConfiguration configuration)
    {
        string connectionString = configuration["CosmosDb"];
        string dataBaseName = configuration["NotificationDBName"];
        string containerName = configuration["NotificationDBContainer"];

        _cosmosClient = new CosmosClient(connectionString);
        _container = _cosmosClient.GetContainer(dataBaseName, containerName);
    }
    
    public async Task<bool> AddNotificationAsync(NotificationEntity notification)
    {
        notification.Id = Guid.NewGuid().ToString();
        notification.NotificationDate = DateTime.UtcNow;
        
        var response = await _container.CreateItemAsync(notification, new PartitionKey(notification.CorrelationId));
        if(response.StatusCode == HttpStatusCode.Created)
            return true;
        return false;
    }

    public async Task<List<NotificationEntity>> GetAllNotificationsAsync()
    {
        var query = _container.GetItemQueryIterator<NotificationEntity>("SELECT * FROM c");
        var notifications = new List<NotificationEntity>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            notifications.AddRange(response.Resource);
        }

        return notifications;
    }
}