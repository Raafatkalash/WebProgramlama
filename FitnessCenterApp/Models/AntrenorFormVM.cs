using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class AntrenorFormVM
    {
        public int? Id { get; set; }

        [Required, StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string UzmanlikAlani { get; set; } = string.Empty;

        [Phone, StringLength(20)]
        public string? Telefon { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [Required]
        public int SalonId { get; set; }

        // ✅ IDs للخدمات المختارة
        public List<int> HizmetIds { get; set; } = new();

        [Required]
        public TimeSpan MusaitBaslangic { get; set; }

        [Required]
        public TimeSpan MusaitBitis { get; set; }
    }
}
