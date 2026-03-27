using System.ComponentModel.DataAnnotations;
using StockifyPlus.Models.Enums;

namespace StockifyPlus.Models
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Kullanıcı adı 3 ile 100 karakter arasında olmalıdır.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "�?ifre boş bırakılamaz.")]
        [StringLength(256)]
        public string PasswordHash { get; set; }
        [StringLength(100)]
        public string FullName { get; set; }
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }
        [Required]
        public UserRole Role { get; set; } = UserRole.DepoPersoneli;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastModifiedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}


