
using System.ComponentModel.DataAnnotations;

namespace AutoService.MVC.Models
{
    public class CustomerViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string PhoneNumber { get; set; } = "";
    }

    public class CustomerPageViewModel
    {
        public List<CustomerViewModel> Items { get; set; } = new();
        public string Search { get; set; } = "";
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
    }
}
