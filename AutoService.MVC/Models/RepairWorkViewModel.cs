using System.ComponentModel.DataAnnotations;

namespace AutoService.MVC.Models
{
    public class RepairWorkViewModel
    {
        public int RepairOrderId { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = "";

        [Range(typeof(decimal), "0.01", "1000")]
        public decimal HoursWorked { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PartId { get; set; }

        [Range(1, 10000)]
        public int Quantity { get; set; } = 1;
    }
}
