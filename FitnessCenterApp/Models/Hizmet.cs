using System.Collections.Generic;

namespace FitnessCenterApp.Models
{
    public class Hizmet
    {
        public int Id { get; set; }
        public string Ad { get; set; }          // Örn: "Fitness", "Yoga", "Pilates"
        public string Aciklama { get; set; }
        public decimal Ucret { get; set; }      // Saatlik / seans ücreti

        public int SalonId { get; set; }
        public Salon Salon { get; set; }

        public ICollection<Randevu> Randevular { get; set; }
    }
}
