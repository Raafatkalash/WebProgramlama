using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Antrenor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(100)]
        public string UzmanlikAlani { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Telefon { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}
