
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Infrastructure.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly AppDbContext _context;

        public CarRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Car>> GetAllAsync()
        {
            return await _context.Cars
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Car?> GetByIdAsync(int id)
        {
            return await _context.Cars.FindAsync(id);
        }

        public async Task<List<Car>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task AddAsync(Car car)
        {
            await _context.Cars.AddAsync(car);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Car car)
        {
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
                return;

            bool hasRepairs = await _context.RepairOrders
                .AnyAsync(r => r.CarId == id);

            if (hasRepairs)
                throw new InvalidOperationException(
                    "Ei saa kustuta auto");

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
        }
    }
}
