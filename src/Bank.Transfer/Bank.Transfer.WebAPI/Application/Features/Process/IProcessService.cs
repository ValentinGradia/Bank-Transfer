namespace Bank.Transfer.WebAPI.Application.Features.Process;

public interface IProcessService
{
    Task Execute(string message, string subscription);
}
