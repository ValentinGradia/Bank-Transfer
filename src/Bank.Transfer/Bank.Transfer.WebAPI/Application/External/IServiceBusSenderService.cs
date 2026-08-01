namespace Bank.Transfer.WebAPI.Application.External;

public interface IServiceBusSenderService
{
    Task Execute(object eventModel, string subscriptionName);
}
