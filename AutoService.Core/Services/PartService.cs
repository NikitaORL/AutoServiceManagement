
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;

namespace AutoService.Core.Services
{
    public class PartService : IPartService
    {
        private readonly IPartRepository _repository;

        public PartService(IPartRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Part>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Part?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Part> CreateAsync(Part part)
        {
            if (string.IsNullOrWhiteSpace(part.Name))
                throw new ArgumentException("Name is required");

            if (string.IsNullOrWhiteSpace(part.PartNumber))
                throw new ArgumentException("Part number is required");

            if (part.Price < 0)
                throw new ArgumentException("Price cannot be negative");

            if (part.StockQuantity < 0)
                throw new ArgumentException("Stock cannot be negative");

            part.Id = 0;

            await _repository.AddAsync(part);

            return part;
        }

        public async Task<bool> UpdateAsync(int id, Part part)
        {
            var existingPart = await _repository.GetByIdAsync(id);

            if (existingPart == null)
                return false;

            if (string.IsNullOrWhiteSpace(part.Name))
                throw new ArgumentException("Name is required");

            if (string.IsNullOrWhiteSpace(part.PartNumber))
                throw new ArgumentException("Part number is required");

            if (part.Price < 0)
                throw new ArgumentException("Price cannot be negative");

            if (part.StockQuantity < 0)
                throw new ArgumentException("Stock cannot be negative");

            existingPart.Name = part.Name;
            existingPart.PartNumber = part.PartNumber;
            existingPart.Price = part.Price;
            existingPart.StockQuantity = part.StockQuantity;
            existingPart.Description = part.Description;

            await _repository.UpdateAsync(existingPart);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var part = await _repository.GetByIdAsync(id);

            if (part == null)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}
