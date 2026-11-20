
using System.Text.Json.Serialization;

namespace AutoService.Core.Entities
{
    public class RepairOrder
    {
        public int Id { get; set; }

        public int CarId { get; set; }

        [JsonIgnore]
        public Car? Car { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [JsonIgnore]
        public List<RepairOrderPart> Parts { get; set; } = new();

        [JsonIgnore]
        public List<WorkLog> WorkLogs { get; set; } = new();
    }
}
