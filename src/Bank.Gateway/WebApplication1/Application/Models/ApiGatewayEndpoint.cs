using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Application.Models;

public static class ApiGatewayEndpoint
{
    public static void GatewayEndpoint(WebApplication app)
    {
        //Agregamos un endpoint para el API Gateway
        app.MapPost("/api-gateway", ([FromBody] EndPointModel model) =>
        {
            return model;
        });
        
    }
}