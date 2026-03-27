using System.ComponentModel.DataAnnotations;

namespace StockifyPlus.Models.ViewModels
{
    public class NotificationSettingsViewModel
    {
        public bool PushEnabled { get; set; }

        [Display(Name = "Kritik stok e-posta adresi")]
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        public string AlertEmail { get; set; } = string.Empty;
    }
}

