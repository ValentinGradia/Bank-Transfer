using Bank.Transfer.WebAPI.Application.Database;
using Bank.Transfer.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Transfer.WebAPI.Persistence.Database;

public class DatabaseService : DbContext, IDatabaseService //Implement the IDatabaseService interface to provide independency injection
{
    public DatabaseService(DbContextOptions<DatabaseService> options) : base(options)
    {
    }

    public DbSet<TransferEntity> Transfer { get; set; } 

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