
namespace AutoService.Core.Entities
{
    public class WorkLog
    {
        public int Id { get; set; }

        public int RepairOrderId { get; set; }

        public RepairOrder RepairOrder { get; set; } = null!;

        public string MechanicUserId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal HoursWorked { get; set; }

        public decimal HourlyRate { get; set; }

        public DateTime WorkDate { get; set; } = DateTime.UtcNow;
    }
}
