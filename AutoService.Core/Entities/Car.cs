
using System.Text.Json.Serialization;

namespace AutoService.Core.Entities
{
    public class Car
    {
        public int Id { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public string LicensePlate { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        [JsonIgnore]
        public Customer? Customer { get; set; }

        [JsonIgnore]
        public List<RepairOrder> RepairOrders { get; set; } = new();
    }
}
