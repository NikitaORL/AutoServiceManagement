
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

        public async Task<Customer> CreateAsync(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.FirstName))
                throw new ArgumentException("First name is required");

            if (string.IsNullOrWhiteSpace(customer.LastName))
                throw new ArgumentException("Last name is required");

            await _repository.AddAsync(customer);

            return customer;
        }

        public async Task<bool> UpdateAsync(int id, Customer customer)
        {
            var existingCustomer = await _repository.GetByIdAsync(id);

            if (existingCustomer == null)
                return false;

            if (string.IsNullOrWhiteSpace(customer.FirstName))
                throw new ArgumentException("First name is required");

            if (string.IsNullOrWhiteSpace(customer.LastName))
                throw new ArgumentException("Last name is required");

            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.PhoneNumber = customer.PhoneNumber;

            await _repository.UpdateAsync(existingCustomer);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _repository.GetByIdAsync(id);

            if (customer == null)
                return false;

            if (await _repository.HasCarsAsync(id))
                throw new InvalidOperationException(
                    "Cannot delete a customer who has registered cars.");

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}
