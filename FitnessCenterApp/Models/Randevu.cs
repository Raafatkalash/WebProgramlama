using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FitnessCenterApp.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Üye")]
        public int UyeId { get; set; }

        [ValidateNever]
        public Uye Uye { get; set; } = null!;

        [Required]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        [ValidateNever]
        public Antrenor Antrenor { get; set; } = null!;

        [Required]
        [Display(Name = "Salon")]
        public int SalonId { get; set; }

        [ValidateNever]
        public Salon Salon { get; set; } = null!;

        [Required]
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }

        [ValidateNever]
        public Hizmet Hizmet { get; set; } = null!;

        [Required]
        [Display(Name = "Tarih & Saat")]
        [DataType(DataType.DateTime)]
        public DateTime TarihSaat { get; set; }

        // ✅ محسوبة بالسيرفر
        [BindNever]
        public DateTime BitisTarihSaat { get; set; }

        [StringLength(500)]
        [Display(Name = "Not")]
        public string? Not { get; set; }

        [Display(Name = "Onaylı")]
        public bool Onayli { get; set; }

        [Display(Name = "İptal Edildi")]
        public bool IptalEdildi { get; set; }

        // ✅ Snapshot محسوب بالسيرفر
        [BindNever, StringLength(100)]
        [Display(Name = "Hizmet Adı")]
        public string HizmetAdi { get; set; } = string.Empty;

        [BindNever]
        [Display(Name = "Süre (dk)")]
        public int HizmetSureDakika { get; set; }

        [BindNever]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ücret")]
        public decimal HizmetUcret { get; set; }
    }
}
