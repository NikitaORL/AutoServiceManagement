
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface ICarService
    {
        Task<List<Car>> GetAllAsync();

        Task<Car?> GetByIdAsync(int id);

        Task<List<Car>> GetByCustomerIdAsync(int customerId);

        Task<Car> CreateAsync(Car car);

        Task<bool> UpdateAsync(int id, Car car);

        Task<bool> DeleteAsync(int id);
    }
}
