
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface IPartRepository
    {
        Task<List<Part>> GetAllAsync();

        Task<Part?> GetByIdAsync(int id);

        Task AddAsync(Part part);

        Task UpdateAsync(Part part);

        Task DeleteAsync(int id);
    }
}
