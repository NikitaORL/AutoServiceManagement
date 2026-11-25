using AutoService.Core.DTO;
using Microsoft.AspNetCore.Authorization;
﻿
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoService.Controllers
{
    [Authorize(Roles = "Admin,Service")]
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string sort = "name",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "Page must be positive and pageSize must be 1-100" });

            if (sort != "name" && sort != "email" && sort != "id")
                return BadRequest(new { message = "Unsupported sort field" });

            var customers = await _customerService.GetAllAsync();
            var query = customers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) || c.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) || c.Email.Contains(search, StringComparison.OrdinalIgnoreCase));

            query = sort switch
            {
                "name" => query.OrderBy(c => c.LastName).ThenBy(x => x.Id),
                "email" => query.OrderBy(c => c.Email).ThenBy(x => x.Id),
                _ => query.OrderBy(x => x.Id)
            };

            int total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { items = items.Select(c => new CustomerResponse(c.Id, c.FirstName, c.LastName, c.Email, c.PhoneNumber)), total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
        }

        // GET: api/customers/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);

            if (customer == null)
                return NotFound();

            return Ok(new CustomerResponse(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber));
        }

        // POST: api/customers
        [HttpPost]
        public async Task<ActionResult<Customer>> Create(
            [FromBody] CustomerRequest request)
        {
            var customer = new Customer { FirstName = request.FirstName, LastName = request.LastName, Email = request.Email, PhoneNumber = request.PhoneNumber };
            try
            {
                var createdCustomer =
                    await _customerService.CreateAsync(customer);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdCustomer.Id },
                    new CustomerResponse(createdCustomer.Id, createdCustomer.FirstName, createdCustomer.LastName, createdCustomer.Email, createdCustomer.PhoneNumber));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // PUT: api/customers/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CustomerRequest request)
        {
            var customer = new Customer { FirstName = request.FirstName, LastName = request.LastName, Email = request.Email, PhoneNumber = request.PhoneNumber };
            try
            {
                var updated = await _customerService.UpdateAsync(id, customer);

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        // DELETE: api/customers/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _customerService.DeleteAsync(id);

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
