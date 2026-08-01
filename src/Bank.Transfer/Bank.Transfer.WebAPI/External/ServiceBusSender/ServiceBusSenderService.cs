using Azure.Messaging.ServiceBus;
using Bank.Transfer.WebAPI.Application.External;

namespace Bank.Transfer.WebAPI.External.ServiceBusSender;

public class ServiceBusSenderService : IServiceBusSenderService
{
    private readonly ServiceBusClient _client;
    private readonly string _topicName;

    public ServiceBusSenderService(IConfiguration configuration)        
    {
        _client = new ServiceBusClient(configuration["SERVICEBUSCONSTR"]);
        _topicName = configuration["SERVICEBUSTOPIC"];
    }

    public async Task Execute(object eventModel, string subscriptionName)
    {
        await using Azure.Messaging.ServiceBus.ServiceBusSender sender = _client.CreateSender(_topicName);
        
        string message = System.Text.Json.JsonSerializer.Serialize(eventModel);
        ServiceBusMessage busMessage = new ServiceBusMessage(message);
        
        busMessage.ContentType = "application/json";
        busMessage.Subject = subscriptionName;
        
        await sender.SendMessageAsync(busMessage);
    }
}
