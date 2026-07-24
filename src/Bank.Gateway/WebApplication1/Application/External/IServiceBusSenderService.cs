namespace WebApplication1.Application.External;

public interface IServiceBusSenderService
{
    Task Execute(object eventModel, string subscriptionName);
}