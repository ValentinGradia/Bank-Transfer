using WebApplication1.Application.External;
using WebApplication1.Application.Models;
using WebApplication1.Domain.Constants;

namespace WebApplication1.Application.Features;

public class ProcessService : IProcessService
{
    private readonly IServiceBusSenderService _serviceBusSender;
    
    public ProcessService(IServiceBusSenderService serviceBusSender)
    {
        _serviceBusSender = serviceBusSender;
    }

    public async Task Execute(EndPointModel model)
    {
        var modelEvent = new 
        {
            CorrleationId = Guid.NewGuid().ToString(),
            Amount = model.Amount,
            SourceAccount = model.SourceAccount,
            Destination = model.SourceAccount,
            CustomerId = model.CustomerId
        };
        
        await _serviceBusSender.Execute(model, SendToTopicConstants.TRANSACTION_INITIATED);
    }
}