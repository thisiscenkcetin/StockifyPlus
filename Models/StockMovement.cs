using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StockifyPlus.Models.Enums;

namespace StockifyPlus.Models
{
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Hareket türü seçimi zorunludur.")]
        public MovementType MovementType { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Hareket miktarı 0'dan büyük olmalıdır.")]
        public int Quantity { get; set; }
        [Required]
        public DateTime MovementDate { get; set; } = DateTime.Now;
        [StringLength(500)]
        public string Description { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}


