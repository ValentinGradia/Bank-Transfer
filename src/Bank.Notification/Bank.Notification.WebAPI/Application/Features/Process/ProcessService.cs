using Bank.Notification.WebAPI.Application.Database;
using Bank.Notification.WebAPI.Domain.Constants;
using Bank.Notification.WebAPI.Domain.Entities;
using Newtonsoft.Json;

namespace Bank.Notification.WebAPI.Application.Features.Process;

public class ProcessService : IProcessService
{
    private readonly IDatabaseService _databaseService;

    public ProcessService(IDatabaseService databaseService)
    {
        this._databaseService = databaseService;
    }

    public async Task Execute(string message, string subscription)
    {
        switch (subscription)
        {
            case ReceiveFromTopicConstants.TRANSACTION_COMPLETED:
                await TransactionCompleted(message);
                break;
            case ReceiveFromTopicConstants.TRANSACTION_FAILED:
                await TransactionFailed(message);
                break;
        }
    }

    private async Task TransactionCompleted(string message)
    {
        var entity = JsonConvert.DeserializeObject<NotificationEntity>(message);

        entity.TransactionStatus = true;
        entity.Type = ReceiveFromTopicConstants.TRANSACTION_COMPLETED;
        entity.Content = "Transaction completed successfully";

        await ProcessDatabase(entity);
        await _databaseService.AddNotificationAsync(entity);
    }

    private async Task TransactionFailed(string message)
    {
        var entity = JsonConvert.DeserializeObject<NotificationEntity>(message);

        entity.TransactionStatus = false;
        entity.Type = ReceiveFromTopicConstants.TRANSACTION_FAILED;
        entity.Content = "Transaction failed, please try again.";

        await _databaseService.AddNotificationAsync(entity);
    }
    
    public async Task ProcessDatabase(NotificationEntity entity)
    {
        entity.Type = "email";
        await _databaseService.AddNotificationAsync(entity);
    }
}
