
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
    }
}
