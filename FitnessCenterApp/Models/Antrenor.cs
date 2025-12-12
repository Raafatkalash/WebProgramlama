using System.Collections.Generic;

namespace FitnessCenterApp.Models
{
    public class Antrenor
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string UzmanlikAlani { get; set; }   // Örn: "Kilo verme", "Kas gelişimi"
        public string Telefon { get; set; }

        public int? SalonId { get; set; }
        public Salon Salon { get; set; }

        public ICollection<Randevu> Randevular { get; set; }
    }
}
