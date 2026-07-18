namespace Bank.Transaction.WebAPI.Domain.Entities;

public class TransactionEntity
{
    public int Id { get; set; }
    public string CorrelationId { get; set; } // The identifier of the transaction, this spreads throughout all microservices 
    public DateTime TransactionDate { get; set; }
    public string CurrentState { get; set; } 
    public decimal Amount { get; set; }
    public string SourceAccount { get; set; }
    public string DestinationAccount { get; set; }
    public int CustomerId { get; set; } // The customer who initiated the transaction
}