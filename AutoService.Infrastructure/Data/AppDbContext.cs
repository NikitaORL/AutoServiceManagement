
using AutoService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<RepairOrderPart> RepairOrderParts { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Part>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RepairOrderPart>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WorkLog>()
                .Property(w => w.HourlyRate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WorkLog>()
                .Property(w => w.HoursWorked)
                .HasPrecision(9, 2);
        }

    }
}
