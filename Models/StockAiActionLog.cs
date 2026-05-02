using System.ComponentModel.DataAnnotations;

namespace StockifyPlus.Models
{
    public class StockAiActionLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [Required]
        [StringLength(60)]
        public string ActionType { get; set; } = string.Empty;

        [Required]
        [StringLength(40)]
        public string Status { get; set; } = string.Empty;

        [StringLength(80)]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        [StringLength(80)]
        public string? EntityKey { get; set; }

        [Required]
        [StringLength(1200)]
        public string UserPrompt { get; set; } = string.Empty;

        [Required]
        [StringLength(1600)]
        public string AgentResponse { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Metadata { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

