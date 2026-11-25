using AutoService.Core.DTO;
using Microsoft.AspNetCore.Authorization;
﻿
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoService.Controllers
{
    [Authorize(Roles = "Admin,Mechanic,Service")]
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
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string sort = "name",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "Page must be positive and pageSize must be 1-100" });

            if (sort != "name" && sort != "price" && sort != "id")
                return BadRequest(new { message = "Unsupported sort field" });

            var parts = await _service.GetAllAsync();
            var query = parts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || p.PartNumber.Contains(search, StringComparison.OrdinalIgnoreCase));

            query = sort switch
            {
                "name" => query.OrderBy(p => p.Name).ThenBy(x => x.Id),
                "price" => query.OrderBy(p => p.Price).ThenBy(x => x.Id),
                _ => query.OrderBy(x => x.Id)
            };

            int total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { items = items.Select(p => new PartResponse(p.Id, p.Name, p.PartNumber, p.Price, p.StockQuantity, p.Description)), total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
        }

        // GET: api/parts/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Part>> GetById(int id)
        {
            var part = await _service.GetByIdAsync(id);

            if (part == null)
                return NotFound();

            return Ok(new PartResponse(part.Id, part.Name, part.PartNumber, part.Price, part.StockQuantity, part.Description));
        }


        // POST: api/parts
        [Authorize(Roles = "Admin,Service")]
        [HttpPost]
        public async Task<ActionResult<Part>> Create(
            [FromBody] PartRequest request)
        {
            var part = new Part { Name = request.Name, PartNumber = request.PartNumber, Price = request.Price, StockQuantity = request.StockQuantity, Description = request.Description };
            try
            {
                var createdPart = await _service.CreateAsync(part);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdPart.Id },
                    new PartResponse(createdPart.Id, createdPart.Name, createdPart.PartNumber, createdPart.Price, createdPart.StockQuantity, createdPart.Description));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        // PUT: api/parts/1
        [Authorize(Roles = "Admin,Service")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] PartRequest request)
        {
            var part = new Part { Name = request.Name, PartNumber = request.PartNumber, Price = request.Price, StockQuantity = request.StockQuantity, Description = request.Description };
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
        [Authorize(Roles = "Admin,Service")]
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
