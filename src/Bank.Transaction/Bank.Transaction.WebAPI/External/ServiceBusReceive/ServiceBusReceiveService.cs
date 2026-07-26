using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Bank.Transaction.WebAPI.Domain.Constants;
using Bank.Transaction.WebAPI.Domain.Events;
using MediatR;

namespace Bank.Transaction.WebAPI.External.ServiceBusReceive;

//This class will always be listening to the service bus and will receive the events from the message broker.
public class ServiceBusReceiveService : BackgroundService //Due to the fact that our service will always be listening to the service bus, we will use the BackgroundService class to run the service in the background.
//BackgroundService is a base class for implementing a long-running IHostedService. It provides a convenient way to run background tasks in an ASP.NET Core application.
//When the app starts, it calls ExecuteAsync() (keeps alive until the app is down) method to start the background task. When the app is shutting down, it calls StopAsync() method to stop the background task.
{
    private readonly ServiceBusClient _client;
    private readonly IMediator _mediator;
    private readonly List<ServiceBusProcessor> _processors;
    private string _topicName;

    public ServiceBusReceiveService(IConfiguration configuration, IMediator mediator)
    {
        _mediator = mediator;
        _client = new ServiceBusClient(configuration["SERVICEBUSCONSTR"]);
        _topicName = configuration["SERVICEBUSTOPIC"];

        //Define the subscriptions that we want to listen to.
        var subscriptions = new[]
        {
            ReceiveFromTopicConstants.TRANSACTION_INITIATED,
            ReceiveFromTopicConstants.BALANCE_CONFIRMED,
            ReceiveFromTopicConstants.BALANCE_FAILED,
            ReceiveFromTopicConstants.TRANSFER_CONFIRMED,
            ReceiveFromTopicConstants.TRANSFER_FAILED
        };

        // The subscriptions this service listens to. Each subscription receives messages
        // filtered by event name from the Service Bus topic.
        _processors = subscriptions.Select(subscription =>
        {
            //For each subscription, we create a ServiceBusProcessor (for processing messages) that watches a specific subscription on the topic.
            var processor = _client.CreateProcessor(this._topicName, subscription);

            processor.ProcessMessageAsync += async args => await Process(args, subscription);
            processor.ProcessErrorAsync += ProcessError;

            return processor;
        }).ToList();
    }
    
    //Execute the _processors
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(_processors.Select(p => p.StartProcessingAsync())); //Starts all processors. 
        //When we call StartProcessingAsync() -> Starts receving messages from the subcription and triggers processMessage for each message that arrives
        // All 5 processors start listening simultaneously. Each one watches its own subscription and fires Process() when a           
        //matching message arrives.  
        
        await Task.Run(() => stoppingToken.WaitHandle.WaitOne(), stoppingToken); //Keep the service alive until the app shuts down.
    }

    private Task ProcessError(ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
    
    //Capture the message / event and send it to the mediator to be processed by the appropriate handler.
    private async Task Process(ProcessMessageEventArgs args, string subscription)
    {
        string body = args.Message.Body.ToString();

        await _mediator.Publish(new ProcessEvent(body, subscription));//This reaches ProcessHandler
        await args.CompleteMessageAsync(args.Message); //Tell Service Bus "I processed this message               
        //successfully" — removes it from the queue so it won't be delivered again
    }

    // Called when the application is shutting down. Stops all processors so they
    // stop listening to the Service Bus subscriptions and finish processing any
    // in-flight messages before the service terminates.
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_processors.Select(p => p.StopProcessingAsync()));
        await base.StopAsync(cancellationToken);
    }

    // Releases all resources held by this service: disposes each processor
    // and the ServiceBusClient connection to Azure Service Bus.
    public override async void Dispose()
    {
        await Task.WhenAll(_processors.Select(p => p.DisposeAsync().AsTask()));
        await _client.DisposeAsync();
        base.Dispose();
    }
}