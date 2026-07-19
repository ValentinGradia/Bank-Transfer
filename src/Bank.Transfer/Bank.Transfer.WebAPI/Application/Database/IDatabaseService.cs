using Bank.Transfer.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Transfer.WebAPI.Application.Database;

public interface IDatabaseService
{
    DbSet<TransferEntity> Transfer { get; set; }
    Task<bool> SaveAsync();
}