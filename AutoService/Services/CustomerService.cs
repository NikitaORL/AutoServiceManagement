
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;

namespace AutoService.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public Task<Customer> CreateAsync(Customer customer)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(int id, Customer customer)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
