
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


        // POST: api/repair-orders
        [HttpPost]
        public async Task<ActionResult<RepairOrder>> Create(
            [FromBody] RepairOrder order)
        {
            try
            {
                var createdOrder = await _service.CreateAsync(order);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdOrder.Id },
                    createdOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // PUT: api/repair-orders/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] RepairOrder order)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, order);

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // DELETE: api/repair-orders/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }



    }
}
