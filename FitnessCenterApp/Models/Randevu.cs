using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        // FK (هي اللي نربطها بالـ dropdowns)
        [Required]
        public int UyeId { get; set; }

        [Required]
        public int AntrenorId { get; set; }

        [Required]
        public int SalonId { get; set; }

        [Required]
        public DateTime TarihSaat { get; set; }

        public string? Not { get; set; }

        // Navigation properties (بدون Required لأن الفورم ما يبعتهن)
        public Uye? Uye { get; set; }
        public Antrenor? Antrenor { get; set; }
        public Salon? Salon { get; set; }
    }
}
