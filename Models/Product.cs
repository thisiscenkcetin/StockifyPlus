using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockifyPlus.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Ürün adı boş bırakılamaz.")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "Ürün adı 2 ile 150 karakter arasında olmalıdır.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Stok kodu (SKU) boş bırakılamaz.")]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999.99, ErrorMessage = "Fiyat geçerli bir değer olmalıdır.")]
        public decimal Price { get; set; } = 0;
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz.")]
        public int StockQuantity { get; set; } = 0;
        [Required]
        [Range(0, int.MaxValue)]
        public int CriticalStockLevel { get; set; } = 10;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public ICollection<ProductCustomField> CustomFields { get; set; } = new List<ProductCustomField>();
    }
}


