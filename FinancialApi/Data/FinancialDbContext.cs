using FinancialApi.Models;
using FinancialApi.Models.Account;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace FinancialApi.Data
{
    public class FinancialDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Currency> Currencies { get; set; } = null!;
        public FinancialDbContext(DbContextOptions<FinancialDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Currency>()
        .Property(p => p.Value)
        .HasColumnType("decimal(38,15)");
        }

    }
}
