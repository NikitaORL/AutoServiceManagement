
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoService.Controllers
{
    [ApiController]
    [Route("api/repair-orders")]
    public class RepairOrdersController : ControllerBase
    {
        private readonly IRepairOrderService _service;

        public RepairOrdersController(IRepairOrderService service)
        {
            _service = service;
        }

        // GET: api/repair-orders
        [HttpGet]
        public async Task<ActionResult<List<RepairOrder>>> GetAll()
        {
            var orders = await _service.GetAllAsync();

            return Ok(orders);
        }

        // GET: api/repair-orders/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RepairOrder>> GetById(int id)
        {
            var order = await _service.GetByIdAsync(id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // GET: api/repair-orders/car/1
        [HttpGet("car/{carId:int}")]
        public async Task<ActionResult<List<RepairOrder>>> GetByCarId(int carId)
        {
            var orders = await _service.GetByCarIdAsync(carId);

            return Ok(orders);
        }
    }
}
