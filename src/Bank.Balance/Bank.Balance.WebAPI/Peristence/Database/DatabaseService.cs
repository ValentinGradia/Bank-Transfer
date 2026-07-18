using Bank.Balance.WebAPI.Application.Database;
using Bank.Balance.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Balance.WebAPI.Peristence.Database;

public class DatabaseService : DbContext, IDatabaseService //Implement the IDatabaseService interface to provide independency injection
{
    public DatabaseService(DbContextOptions<DatabaseService> options) : base(options)
    {
    }

    public DbSet<BalanceEntity> Balance { get; set; }

    public async Task<bool> SaveAsync()
    {
        return await SaveChangesAsync() > 0; //Confirm whether the changes were saved successfully
    }
    
    //Create the implementation of the OnModelCreating method to configure the model
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    
}