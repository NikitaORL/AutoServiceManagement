using System.ComponentModel.DataAnnotations;

namespace AutoService.Core.DTO
{
    public class CustomerRequest
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [StringLength(40)]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public record CustomerResponse(
        int Id, string FirstName, string LastName, string Email, string PhoneNumber);

    public class CarRequest
    {
        [Required, StringLength(60)]
        public string Brand { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string Model { get; set; } = string.Empty;

        [Range(1886, 2100)]
        public int Year { get; set; }

        [Required, StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }
    }

    public record CarResponse(
        int Id, string Brand, string Model, int Year, string LicensePlate, int CustomerId);

    public class RepairOrderRequest
    {
        [Range(1, int.MaxValue)]
        public int CarId { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";
    }

    public record RepairOrderResponse(
        int Id, int CarId, string Description, string Status, DateTime CreatedAt, DateTime? CompletedAt);

    public class PartRequest
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string PartNumber { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "999999999")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
    }

    public record PartResponse(
        int Id, string Name, string PartNumber, decimal Price, int StockQuantity, string Description);
}
