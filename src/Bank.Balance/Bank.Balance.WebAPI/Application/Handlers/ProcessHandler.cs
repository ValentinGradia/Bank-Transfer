using Bank.Balance.WebAPI.Application.Features.Process;
using Bank.Balance.WebAPI.Domain.Events;
using MediatR;

namespace Bank.Balance.WebAPI.Application.Handlers;

public class ProcessHandler : INotificationHandler<ProcessEvent>
{
    // MediatR registers handlers as singletons by default.
    // This means ProcessHandler lives for the entire application lifetime.
    // However, IProcessService is registered as scoped (one instance per unit of work).
    // A singleton cannot hold a scoped service directly — it would never be disposed,
    // causing obsolete state and connection leaks (captive dependency problem).
    // The solution: use IServiceProvider to create a new scope per message,
    // resolve the scoped service inside it, and let the scope handle disposal.
    private readonly IServiceProvider _serviceProvider;
    
    public ProcessHandler(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }
    
    public async Task Handle(ProcessEvent notification, CancellationToken cancellationToken)
    {
        // Create a new scope for this message. Each message gets its own
        // fresh IProcessService and DatabaseService, completely isolated.
        using var scope = _serviceProvider.CreateScope();
        var processService = scope.ServiceProvider.GetRequiredService<IProcessService>();
        
        await processService.Execute(notification.Message, notification.Subscription);
        // Scope is disposed here — IProcessService and its dependencies are cleaned up.
    }
}
