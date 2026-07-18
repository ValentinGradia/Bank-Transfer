namespace WebApplication1.Application.Models;

public class EndPointModel
{
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string SourceAccount { get; set; }
    public string DestinationAccount { get; set; }
}