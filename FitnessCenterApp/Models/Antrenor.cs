using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        [NotMapped]
        public string AdSoyad => Ad + " " + Soyad;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(100)]
        public string UzmanlikAlani { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Telefon { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        // ✅ FK للصالة
        [Required]
        public int SalonId { get; set; }

        // ✅ Navigation (لا نعمل له validation من الفورم)
        [ValidateNever]
        public Salon Salon { get; set; } = null!;

        // ✅ علاقات
        [ValidateNever]
        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();

        [ValidateNever]
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        // ✅ ساعات التوفر
        [Required]
        public TimeSpan MusaitBaslangic { get; set; }

        [Required]
        public TimeSpan MusaitBitis { get; set; }
    }
}
