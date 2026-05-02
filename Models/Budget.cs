using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockifyPlus.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "BÃ¼tÃ§e adÄ± boÅŸ bÄ±rakÄ±lamaz.")]
        [StringLength(200, MinimumLength = 2,
            ErrorMessage = "BÃ¼tÃ§e adÄ± 2 ile 200 karakter arasÄ±nda olmalÄ±dÄ±r.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999.99, ErrorMessage = "BÃ¼tÃ§e limiti geÃ§erli bir deÄŸer olmalÄ±dÄ±r.")]
        public decimal MonthlyLimit { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999.99, ErrorMessage = "Harcama geÃ§erli bir deÄŸer olmalÄ±dÄ±r.")]
        public decimal CurrentSpent { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsWarningNotified { get; set; } = false;

        public bool IsCriticalNotified { get; set; } = false;

        [Range(0, 100, ErrorMessage = "UyarÄ± eÅŸiÄŸi 0 ile 100 arasÄ±nda olmalÄ±dÄ±r.")]
        public int WarningThreshold { get; set; } = 80;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastUpdatedDate { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [NotMapped]
        public decimal RemainingBudget => MonthlyLimit - CurrentSpent;

        [NotMapped]
        public decimal SpentPercentage => MonthlyLimit > 0 ? (CurrentSpent / MonthlyLimit) * 100 : 0;

        [NotMapped]
        public bool IsOverBudget => CurrentSpent > MonthlyLimit;

        [NotMapped]
        public bool IsWarningThresholdReached => SpentPercentage >= WarningThreshold;
    }
}

