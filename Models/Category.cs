using System.ComponentModel.DataAnnotations;

namespace StockifyPlus.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
        [StringLength(100, MinimumLength = 2, 
            ErrorMessage = "Kategori adı 2 ile 100 karakter arasında olmalıdır.")]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}


