using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace financing_api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // seed the skill data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Frequency>()
                .Property(f => f.Name)
                .HasConversion<string>();

            modelBuilder.Entity<Recurring>()
                .Property(r => r.Frequency)
                .HasConversion<string>();


        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Frequency> Frequencies { get; set; }
        public DbSet<Recurring> Recurrings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<LoggingTrace> LoggingTrace { get; set; }
        public DbSet<LoggingException> LoggingException { get; set; }
        public DbSet<LoggingDataExchange> LoggingDataExchange { get; set; }
    }
}
