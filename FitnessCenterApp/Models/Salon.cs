using System.Collections.Generic;

namespace FitnessCenterApp.Models
{
    public class Salon
    {
        public int Id { get; set; }                 // Birincil anahtar
        public string Ad { get; set; }              // Salon adı
        public string Adres { get; set; }           // Adres
        public string Telefon { get; set; }         // İletişim telefonu
        public string CalismaSaatleri { get; set; } // Örn: "Hafta içi 09:00-22:00"

        // İlişkiler
        public ICollection<Hizmet> Hizmetler { get; set; }
        public ICollection<Antrenor> Antrenorler { get; set; }
    }
}
