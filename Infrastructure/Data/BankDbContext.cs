using Microsoft.EntityFrameworkCore;
using BankAPI.Domain.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BankAPI.Infrastructure.Data;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
    {
    }
    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasIndex(a => a.AccountNumber)
            .IsUnique();
    }
}