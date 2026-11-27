using System.ComponentModel.DataAnnotations;

namespace AutoService.MVC.Models
{
    public class RepairOrderViewModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int CarId { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; } = "";

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? AssignedMechanicUserId { get; set; }
    }

    public class RepairOrderPageViewModel
    {
        public List<RepairOrderViewModel> Items { get; set; } = new();
        public string Search { get; set; } = "";
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
    }
}
