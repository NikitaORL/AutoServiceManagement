using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoService.Core.Entities;
using AutoService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AutoService.Controllers
{
    public class RegisterRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(12), MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class StaffRequest
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(12), MaxLength(128)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<AppUser> _hasher = new();

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            string email = request.Email.Trim();
            string normalizedEmail = email.ToUpperInvariant();

            if (await _db.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
                return Conflict(new { message = "Email is already registered" });

            await using var transaction = await _db.Database.BeginTransactionAsync();
            var customer = new Customer
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            var user = new AppUser
            {
                Email = email,
                NormalizedEmail = normalizedEmail,
                Role = "Customer",
                CustomerId = customer.Id
            };
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Account could not be registered" });
            }

            return Created("/api/auth/login", new { user.Id, user.Email, user.Role });
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToUpperInvariant();
            var user = await _db.Users
                .SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
                    == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid credentials" });

            var expires = DateTime.UtcNow.AddHours(8);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            if (user.CustomerId.HasValue)
                claims.Add(new Claim("customerId", user.CustomerId.Value.ToString()));

            var keyText = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured");
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt = expires,
                user.Role
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff(StaffRequest request)
        {
            if (request.Role != "Mechanic" && request.Role != "Service")
                return BadRequest(new { message = "Role must be Mechanic or Service" });

            var email = request.Email.Trim();
            var normalizedEmail = email.ToUpperInvariant();
            if (await _db.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
                return Conflict(new { message = "Email is already registered" });

            var user = new AppUser
            {
                Email = email,
                NormalizedEmail = normalizedEmail,
                Role = request.Role
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Account could not be created" });
            }

            return Created("/api/auth/staff", new { user.Id, user.Email, user.Role });
        }
    }
}
