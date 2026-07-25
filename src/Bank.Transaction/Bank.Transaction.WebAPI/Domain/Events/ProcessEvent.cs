using MediatR;

namespace Bank.Transaction.WebAPI.Domain.Events;

// This class recieve the events from the message broker
public class ProcessEvent : INotification
{
    public string Message { get; set; }
    public string Subscription { get; set; }

    public ProcessEvent(string message, string subscription)
    {
        Message = message;
        Subscription = subscription;
    }
    
}