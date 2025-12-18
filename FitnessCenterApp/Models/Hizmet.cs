using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Aciklama { get; set; }

        [Range(1, 1000, ErrorMessage = "SüreDakika en az 1 olmalı.")]
        public int SureDakika { get; set; }

        [Range(0, 1000000, ErrorMessage = "Ücret geçersiz.")]
        public decimal Ucret { get; set; }

        // ✅ هذا هو المهم: الربط يكون بـ SalonId
        [Required(ErrorMessage = "Salon zorunludur.")]
        public int SalonId { get; set; }

        public Salon? Salon { get; set; }

        // علاقات
        public ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
