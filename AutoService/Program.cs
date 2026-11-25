using System.Threading.RateLimiting;
using AutoService.OpenApi;
using AutoService.Hubs;
using AutoService.Services;
using AutoService.Core.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using AutoService.Infrastructure.Data;
using AutoService.Core.Services;
using Microsoft.EntityFrameworkCore;
using AutoService.Core.Interfaces;
using AutoService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configure Jwt:Key with at least 32 UTF-8 bytes using user secrets or environment variables.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/ws/orders"))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Email
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddProblemDetails();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderStatusNotifier, OrderStatusNotifier>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MvcClient", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins != null && origins.Length > 0)
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));




builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();

builder.Services.AddScoped<IRepairOrderRepository, RepairOrderRepository>();
builder.Services.AddScoped<IRepairOrderService, RepairOrderService>();

builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IPartService, PartService>();

builder.Services.AddScoped<IRepairOrderPartRepository, RepairOrderPartRepository>();
builder.Services.AddScoped<IRepairOrderPartService, RepairOrderPartService>();

builder.Services.AddScoped<IWorkLogRepository, WorkLogRepository>();
builder.Services.AddScoped<IWorkLogService, WorkLogService>();

builder.Services.AddScoped<IInvoiceService>(sp =>
    new InvoiceService(
        sp.GetRequiredService<IRepairOrderRepository>(),
        sp.GetRequiredService<IRepairOrderPartService>(),
        sp.GetRequiredService<IWorkLogService>(),
        builder.Configuration.GetValue<decimal>("Invoice:VatRate")));






// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// Apply migrations and create the first admin only when credentials are provided
// through environment variables or user secrets.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var adminEmail = builder.Configuration["BootstrapAdmin:Email"]?.Trim();
    var adminPassword = builder.Configuration["BootstrapAdmin:Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        if (adminPassword.Length < 12)
            throw new InvalidOperationException("Bootstrap admin password must be at least 12 characters.");

        if (!await db.Users.AnyAsync(u => u.Role == "Admin"))
        {
            var admin = new AppUser
            {
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpperInvariant(),
                Role = "Admin"
            };
            admin.PasswordHash = new PasswordHasher<AppUser>().HashPassword(admin, adminPassword);
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "AutoService API v1");
    });
}


app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("MvcClient");
app.UseRateLimiter();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/ws/orders");

app.Run();
