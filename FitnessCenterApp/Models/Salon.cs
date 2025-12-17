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
    }
}
