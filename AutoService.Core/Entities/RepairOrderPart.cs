
namespace AutoService.Core.Entities
{
    public class RepairOrderPart
    {
        public int Id { get; set; }

        public int RepairOrderId { get; set; }
        public RepairOrder RepairOrder { get; set; } = null!;

        public int PartId { get; set; }
        public Part Part { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public List<RepairOrderPart> Parts { get; set; } = new();
    }
}
