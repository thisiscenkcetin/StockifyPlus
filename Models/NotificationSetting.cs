using System.ComponentModel.DataAnnotations;

namespace StockifyPlus.Models
{
    public class NotificationSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public bool PushEnabled { get; set; } = true;

        [StringLength(100)]
        [EmailAddress]
        public string AlertEmail { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastUpdatedBy { get; set; } = "System";

        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}

