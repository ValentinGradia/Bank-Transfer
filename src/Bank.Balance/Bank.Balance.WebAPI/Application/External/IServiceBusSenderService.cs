namespace Bank.Balance.WebAPI.Application.External;

public interface IServiceBusSenderService
{
    Task Execute(object eventModel, string subscriptionName);
}
