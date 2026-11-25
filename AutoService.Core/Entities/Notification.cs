namespace AutoService.Core.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
