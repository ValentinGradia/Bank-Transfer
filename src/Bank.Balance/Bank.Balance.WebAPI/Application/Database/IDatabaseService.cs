using Bank.Balance.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Balance.WebAPI.Application.Database;

public interface IDatabaseService
{
    DbSet<BalanceEntity> Balance { get; set; }
    Task<bool> SaveAsync();
}