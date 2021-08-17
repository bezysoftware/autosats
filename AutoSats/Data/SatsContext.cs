using AutoSats.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AutoSats.Data
{
    public class SatsContext : DbContext
    {
        public SatsContext()
        {
        }

        public SatsContext(DbContextOptions<SatsContext> options) : base(options)
        {
        }

        public DbSet<ExchangeEvent> ExchangeEvents { get; set; }
        public DbSet<ExchangeEventBuy> ExchangeBuys { get; set; }
        public DbSet<ExchangeEventWithdrawal> ExchangeWithdrawals { get; set; }
        public DbSet<ExchangeSchedule> ExchangeSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetEnumConverterForAll();

            modelBuilder.Entity<ExchangeEvent>().ToTable("ExchangeEvents");
            modelBuilder.Entity<ExchangeEventBuy>().ToTable("ExchangeBuys");
            modelBuilder.Entity<ExchangeEventWithdrawal>().ToTable("ExchangeWithdrawals");
        }
    }
}
