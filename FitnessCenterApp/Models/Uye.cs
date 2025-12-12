using System;
using System.Collections.Generic;

namespace FitnessCenterApp.Models
{
    public class Uye
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }

        public DateTime UyelikBaslangic { get; set; } = DateTime.Now;
        public bool AktifMi { get; set; } = true;

        public ICollection<Randevu> Randevular { get; set; }
    }
}
