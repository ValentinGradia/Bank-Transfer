namespace Bank.Transaction.WebAPI.Application.Features.Process;

public interface IProcessService
{
    Task Execute(string message, string subscription);
}