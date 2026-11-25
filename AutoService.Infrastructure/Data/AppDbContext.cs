
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
        public DbSet<AppUser> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .Property(u => u.NormalizedEmail)
                .HasMaxLength(256);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Email)
                .HasMaxLength(256);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Role)
                .HasMaxLength(32);

            modelBuilder.Entity<AppUser>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(u => u.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

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

            ////////////////////////////////////
            modelBuilder.Entity<Part>().HasData(
                new Part
                {
                    Id = 1,
                    Name = "Oil Filter",
                    PartNumber = "OIL-001",
                    Price = 12.50m,
                    StockQuantity = 15,
                    Description = "Engine oil filter"
                },
                new Part
                {
                    Id = 2,
                    Name = "Brake Pads",
                    PartNumber = "BRK-001",
                    Price = 45.00m,
                    StockQuantity = 8,
                    Description = "Front brake pads"
                },
                new Part
                {
                    Id = 3,
                    Name = "Engine Oil 1L",
                    PartNumber = "ENG-001",
                    Price = 14.00m,
                    StockQuantity = 30,
                    Description = "Synthetic engine oil"
                }
            );

        }

    }
}
