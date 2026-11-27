namespace AutoService.MVC.Models
{
    public class MyCarViewModel
    {
        public int Id { get; set; }
        public string Brand { get; set; } = "";
        public string Model { get; set; } = "";
        public int Year { get; set; }
        public string LicensePlate { get; set; } = "";
        public DateTime? NextServiceDate { get; set; }
    }

    public class MyOrderViewModel
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class MyNotificationViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class MyAccountViewModel
    {
        public List<MyCarViewModel> Cars { get; set; } = new();
        public List<MyOrderViewModel> Orders { get; set; } = new();
        public List<MyNotificationViewModel> Notifications { get; set; } = new();
    }

    public class MyInvoiceViewModel
    {
        public int RepairOrderId { get; set; }
        public decimal PartsSubtotal { get; set; }
        public decimal LaborSubtotal { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }
    }
}
