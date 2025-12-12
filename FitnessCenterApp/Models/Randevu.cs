using System;

namespace FitnessCenterApp.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        public int UyeId { get; set; }
        public Uye Uye { get; set; }

        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; }

        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; }

        public DateTime TarihSaat { get; set; }   // Randevu başlangıç zamanı
        public int SureDakika { get; set; } = 60; // Varsayılan 60 dakika

        public string Durum { get; set; } = "Beklemede";
        // "Beklemede", "Onaylandı", "İptal"
    }
}
