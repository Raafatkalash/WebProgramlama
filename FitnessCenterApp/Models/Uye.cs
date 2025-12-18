using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models
{
    public class Uye
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string Telefon { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [NotMapped]
        public string AdSoyad => $"{Ad} {Soyad}";

        // ✅ مواعيد العضو (مفيد + يساعد بالتعارض)
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
