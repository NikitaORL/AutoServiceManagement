
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoService.Controllers
{
    [ApiController]
    [Route("api/cars")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        // GET: api/cars
        [HttpGet]
        public async Task<ActionResult<List<Car>>> GetAll()
        {
            var cars = await _carService.GetAllAsync();

            return Ok(cars);
        }

        // GET: api/cars/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Car>> GetById(int id)
        {
            var car = await _carService.GetByIdAsync(id);

            if (car == null)
                return NotFound();

            return Ok(car);
        }

        // GET: api/cars/customer/1
        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<List<Car>>> GetByCustomerId(
            int customerId)
        {
            var cars = await _carService.GetByCustomerIdAsync(customerId);

            return Ok(cars);
        }
    }
}
