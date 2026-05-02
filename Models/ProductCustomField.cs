using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockifyPlus.Models
{
    public class ProductCustomField
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Alan adÄ± boÅŸ bÄ±rakÄ±lamaz.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Alan adÄ± 2 ile 100 karakter arasÄ±nda olmalÄ±dÄ±r.")]
        public string FieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alan deÄŸeri boÅŸ bÄ±rakÄ±lamaz.")]
        [StringLength(500)]
        public string FieldValue { get; set; } = string.Empty;

        [StringLength(20)]
        public string FieldType { get; set; } = "Text";

        [StringLength(20)]
        public string? Unit { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}

