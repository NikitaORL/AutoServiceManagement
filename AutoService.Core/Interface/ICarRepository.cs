
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface ICarRepository
    {
        Task<List<Car>> GetAllAsync();

        Task<Car?> GetByIdAsync(int id);

        Task<List<Car>> GetByCustomerIdAsync(int customerId);

        Task AddAsync(Car car);

        Task UpdateAsync(Car car);

        Task DeleteAsync(int id);
    }
}
