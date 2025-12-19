using System.Linq;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Uye> Uyeler { get; set; } = null!;
        public DbSet<Antrenor> Antrenorler { get; set; } = null!;
        public DbSet<Salon> Salonlar { get; set; } = null!;
        public DbSet<Hizmet> Hizmetler { get; set; } = null!;
        public DbSet<Randevu> Randevular { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ حل شامل لمشكلة multiple cascade paths
            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // ✅ many-to-many بين Antrenor و Hizmet
            modelBuilder.Entity<Antrenor>()
                .HasMany(a => a.Hizmetler)
                .WithMany(h => h.Antrenorler);
            modelBuilder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasPrecision(18, 2);

        }
    }
}
