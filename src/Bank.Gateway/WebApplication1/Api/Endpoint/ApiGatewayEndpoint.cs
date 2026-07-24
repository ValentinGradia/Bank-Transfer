using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Features;

namespace WebApplication1.Application.Models;

public static class ApiGatewayEndpoint
{
    public static void GatewayEndpoint(WebApplication app)
    {
        //Agregamos un endpoint para el API Gateway
        app.MapPost("/api-gateway", async ([FromBody] EndPointModel model, [FromServices] IProcessService processService) =>
        {
            await processService.Execute(model);
            return model;
        });
        
    }
}