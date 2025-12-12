using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Models;

namespace FitnessCenterApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Veritabanındaki tablolar
        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Uye> Uyeler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
    }
}
