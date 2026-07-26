using Bank.Transaction.WebAPI.Domain.Constants;

namespace Bank.Transaction.WebAPI.Application.Features.Process;

public class ProcessService : IProcessService
{
    public ProcessService()
    {
        
    }

    //This method is responsible for reciving events and processing them calling the appropiate method.  
    public async Task Execute(string message, string subscription)
    {
        switch (subscription)
        {
            case ReceiveFromTopicConstants.TRANSACTION_INITIATED:
                await TransactionInitiated(message);
                break;
            case ReceiveFromTopicConstants.BALANCE_CONFIRMED:
                await BalanceConfirmed(message);
                break;
            case ReceiveFromTopicConstants.BALANCE_FAILED:
                await BalanceFailed(message);
                break;
            case ReceiveFromTopicConstants.TRANSFER_CONFIRMED:
                await TransferConfirmed(message);
                break;
            case ReceiveFromTopicConstants.TRANSFER_FAILED:
                await TransferFailed(message);
                break;
        }
    }

    private async Task TransactionInitiated(string message)
    {
        
    }

    private async Task BalanceConfirmed(string message)
    {
        
    }

    private async Task BalanceFailed(string message)
    {
        
    }

    private async Task TransferConfirmed(string message)
    {
        
    }

    private async Task TransferFailed(string message)
    {
        
    }
}