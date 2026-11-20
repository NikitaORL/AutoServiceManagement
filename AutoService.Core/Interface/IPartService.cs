
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface IPartService
    {
        Task<List<Part>> GetAllAsync();

        Task<Part?> GetByIdAsync(int id);

        Task<Part> CreateAsync(Part part);

        Task<bool> UpdateAsync(int id, Part part);

        Task<bool> DeleteAsync(int id);
    }
}
