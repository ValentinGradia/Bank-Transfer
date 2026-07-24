using WebApplication1.Application.Models;

namespace WebApplication1.Application.Features;

public interface IProcessService
{
    Task Execute(EndPointModel model);
}