
using System.Text.Json.Serialization;

namespace AutoService.Core.Entities
{
    public class Part
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string PartNumber { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string Description { get; set; } = string.Empty;

        [JsonIgnore]
        public List<RepairOrderPart> RepairOrderParts { get; set; } = new();
    }
}
