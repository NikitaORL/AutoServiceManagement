using System.ComponentModel.DataAnnotations;

namespace AutoService.MVC.Models
{
    public class CarViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(60)]
        public string Brand { get; set; } = "";

        [Required, StringLength(60)]
        public string Model { get; set; } = "";

        [Range(1886, 2100)]
        public int Year { get; set; }

        [Required, StringLength(20)]
        public string LicensePlate { get; set; } = "";

        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NextServiceDate { get; set; }
    }

    public class CarPageViewModel
    {
        public List<CarViewModel> Items { get; set; } = new();
        public string Search { get; set; } = "";
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
    }
}
