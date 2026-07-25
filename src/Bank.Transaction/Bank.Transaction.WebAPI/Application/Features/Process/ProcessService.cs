namespace Bank.Transaction.WebAPI.Application.Features.Process;

public class ProcessService : IProcessService
{
    public ProcessService()
    {
        
    }

    //This method is responsible for reciving events and processing them calling the appropiate handler.  
    public async Task Execute(string message, string subscription)
    {
        
    }
}