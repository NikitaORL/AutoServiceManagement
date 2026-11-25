
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

        // POST: api/cars
        [HttpPost]
        public async Task<ActionResult<Car>> Create(
            [FromBody] Car car)
        {
            try
            {
                var createdCar = await _carService.CreateAsync(car);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdCar.Id },
                    createdCar);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // PUT: api/cars/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] Car car)
        {
            try
            {
                var updated = await _carService.UpdateAsync(id, car);

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        // DELETE: api/cars/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _carService.DeleteAsync(id);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }




        // GET: api/cars/1/history
        [HttpGet("{id:int}/history")]
        public async Task<IActionResult> GetHistory(
            int id,
            [FromServices] IRepairOrderService repairOrders)
        {
            var car = await _carService.GetByIdAsync(id);

            if (car == null)
                return NotFound();

            var history = await repairOrders.GetByCarIdAsync(id);

            return Ok(history.OrderByDescending(r => r.CreatedAt));
        }

    }
}
  