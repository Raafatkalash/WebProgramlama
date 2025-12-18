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
        public int UyeId { get; set; }
        [ValidateNever]
        public Uye Uye { get; set; } = null!;

        [Required]
        public int AntrenorId { get; set; }
        [ValidateNever]
        public Antrenor Antrenor { get; set; } = null!;

        [Required]
        public int SalonId { get; set; }
        [ValidateNever]
        public Salon Salon { get; set; } = null!;

        [Required]
        public int HizmetId { get; set; }
        [ValidateNever]
        public Hizmet Hizmet { get; set; } = null!;

        // المستخدم يدخلها من الفورم
        [Required]
        public DateTime TarihSaat { get; set; }

        // ✅ محسوبة بالسيرفر (لا Required ولا binding)
        [BindNever]
        public DateTime BitisTarihSaat { get; set; }

        [StringLength(500)]
        public string? Not { get; set; }

        public bool Onayli { get; set; }
        public bool IptalEdildi { get; set; }

        // ✅ Snapshot محسوب بالسيرفر
        [BindNever, StringLength(100)]
        public string HizmetAdi { get; set; } = string.Empty;

        [BindNever]
        public int HizmetSureDakika { get; set; }

        [BindNever]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HizmetUcret { get; set; }
    }
}
