
namespace AutoService.Core.Entities
{
    public class RepairOrder
    {
        public int Id { get; set; }

        public int CarId { get; set; }

        public Car Car { get; set; } = null!;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }
}
