
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoService.Controllers
{
    [ApiController]
    [Route("api/parts")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _service;

        public PartsController(IPartService service)
        {
            _service = service;
        }

        // GET: api/parts
        [HttpGet]
        public async Task<ActionResult<List<Part>>> GetAll()
        {
            var parts = await _service.GetAllAsync();

            return Ok(parts);
        }

        // GET: api/parts/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Part>> GetById(int id)
        {
            var part = await _service.GetByIdAsync(id);

            if (part == null)
                return NotFound();

            return Ok(part);
        }


        // POST: api/parts
        [HttpPost]
        public async Task<ActionResult<Part>> Create(
            [FromBody] Part part)
        {
            try
            {
                var createdPart = await _service.CreateAsync(part);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdPart.Id },
                    createdPart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        // PUT: api/parts/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] Part part)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, part);

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        // DELETE: api/parts/1
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
