
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
    }
}
