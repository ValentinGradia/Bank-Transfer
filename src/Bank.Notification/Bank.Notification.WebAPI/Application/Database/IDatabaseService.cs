using Bank.Notification.WebAPI.Domain.Entities;

namespace Bank.Notification.WebAPI.Application.Database;

public interface IDatabaseService
{
    Task<bool> AddNotificationAsync(NotificationEntity notification);

    Task<List<NotificationEntity>> GetAllNotificationsAsync();
}