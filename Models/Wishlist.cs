using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockifyPlus.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ÃœrÃ¼n adÄ± boÅŸ bÄ±rakÄ±lamaz.")]
        [StringLength(200, MinimumLength = 2,
            ErrorMessage = "ÃœrÃ¼n adÄ± 2 ile 200 karakter arasÄ±nda olmalÄ±dÄ±r.")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999.99, ErrorMessage = "Hedef fiyat geÃ§erli bir deÄŸer olmalÄ±dÄ±r.")]
        public decimal? TargetPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999.99, ErrorMessage = "Mevcut fiyat geÃ§erli bir deÄŸer olmalÄ±dÄ±r.")]
        public decimal? CurrentPrice { get; set; }

        [Range(1, 4, ErrorMessage = "Ã–ncelik 1 ile 4 arasÄ±nda olmalÄ±dÄ±r.")]
        public int Priority { get; set; } = 2;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(500)]
        [Url(ErrorMessage = "GeÃ§erli bir URL giriniz.")]
        public string? ProductUrl { get; set; }

        public bool IsNotified { get; set; } = false;

        public bool IsPurchased { get; set; } = false;

        public DateTime? PurchaseDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastUpdatedDate { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = null!;
    }
}

