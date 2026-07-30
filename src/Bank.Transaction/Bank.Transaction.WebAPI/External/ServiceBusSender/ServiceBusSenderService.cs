using Azure.Messaging.ServiceBus;
using Bank.Transaction.WebAPI.Application.External;

namespace Bank.Transaction.WebAPI.External.ServiceBusSender;

public class ServiceBusSenderService : IServiceBusSenderService
{
    private readonly ServiceBusClient _client;
    private readonly string _topicName;

    public ServiceBusSenderService(IConfiguration configuration)        
    {
        _client = new ServiceBusClient(configuration["SERVICEBUSCONSTR"]);
        _topicName = configuration["SERVICEBUSTOPIC"];
    }

    //The eventModel is the object that we want to send to the topic
    //The method is independent of the transactional process, this service is aimed to receive the event and save / send.
    public async Task Execute(object eventModel, string subscriptionName)
    {
        //We create a sender to send messages to the topic
        await using Azure.Messaging.ServiceBus.ServiceBusSender sender = _client.CreateSender(_topicName);
        
        //We serialize the event model to JSON and create a ServiceBusMessage
        string message = System.Text.Json.JsonSerializer.Serialize(eventModel);
        ServiceBusMessage busMessage = new ServiceBusMessage(message);
        
        busMessage.ContentType = "application/json";
        busMessage.Subject = subscriptionName; //In the subject we will put the subscription name, so that the subscribers can filter the messages by subject.
        
        await sender.SendMessageAsync(busMessage);
    }
}