
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;

namespace AutoService.Core.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly ICustomerRepository _customerRepository;

        public CarService(
            ICarRepository carRepository,
            ICustomerRepository customerRepository)
        {
            _carRepository = carRepository;
            _customerRepository = customerRepository;
        }

        public async Task<List<Car>> GetAllAsync()
        {
            return await _carRepository.GetAllAsync();
        }

        public async Task<Car?> GetByIdAsync(int id)
        {
            return await _carRepository.GetByIdAsync(id);
        }

        public async Task<List<Car>> GetByCustomerIdAsync(int customerId)
        {
            return await _carRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<Car> CreateAsync(Car car)
        {
            if (string.IsNullOrWhiteSpace(car.Brand))
                throw new ArgumentException("Brand is required");

            if (string.IsNullOrWhiteSpace(car.Model))
                throw new ArgumentException("Model is required");

            if (string.IsNullOrWhiteSpace(car.LicensePlate))
                throw new ArgumentException("License plate is required");

            if (car.Year < 1886 || car.Year > DateTime.UtcNow.Year + 1)
                throw new ArgumentException("Invalid car year");

            var customer = await _customerRepository.GetByIdAsync(car.CustomerId);

            if (customer == null)
                throw new ArgumentException("Customer not found");

            await _carRepository.AddAsync(car);

            return car;
        }

        public async Task<bool> UpdateAsync(int id, Car car)
        {
            var existingCar = await _carRepository.GetByIdAsync(id);

            if (existingCar == null)
                return false;

            if (string.IsNullOrWhiteSpace(car.Brand) ||
                string.IsNullOrWhiteSpace(car.Model) ||
                string.IsNullOrWhiteSpace(car.LicensePlate))
                throw new ArgumentException("Invalid car information");

            if (car.Year < 1886 || car.Year > DateTime.UtcNow.Year + 1)
                throw new ArgumentException("Invalid car year");

            var customer = await _customerRepository.GetByIdAsync(car.CustomerId);

            if (customer == null)
                throw new ArgumentException("Customer not found");

            existingCar.Brand = car.Brand;
            existingCar.Model = car.Model;
            existingCar.Year = car.Year;
            existingCar.LicensePlate = car.LicensePlate;
            existingCar.CustomerId = car.CustomerId;

            await _carRepository.UpdateAsync(existingCar);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);

            if (car == null)
                return false;

            await _carRepository.DeleteAsync(id);

            return true;
        }
    }
}
