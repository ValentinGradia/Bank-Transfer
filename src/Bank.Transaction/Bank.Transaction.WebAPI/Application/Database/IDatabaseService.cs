using Bank.Transaction.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Transaction.WebAPI.Application.Database;

public interface IDatabaseService
{
    DbSet<TransactionEntity> Transaction { get; set; }
    Task<bool> SaveAsync();
}