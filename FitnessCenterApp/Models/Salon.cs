using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Salon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(100)]
        public string Sehir { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Adres { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Telefon { get; set; }

        public bool Aktif { get; set; }

        // ✅ ساعات العمل (مطلوبة بالمرجع)
        [Required]
        public TimeSpan AcilisSaati { get; set; }

        [Required]
        public TimeSpan KapanisSaati { get; set; }

        // (اختياري لكن مفيد)
        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
