using FitnessCenterApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    // هذا كنترولر API (ما فيه Views)
    [ApiController]
    [Route("api/[controller]")]
    public class RaporController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RaporController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------
        // 1) Her antrenör için toplam randevu sayısı
        // GET: /api/rapor/antrenor-randevu-sayisi
        // ----------------------------------------------------
        [HttpGet("antrenor-randevu-sayisi")]
        public async Task<IActionResult> AntrenorRandevuSayisi()
        {
            var sonuc = await _context.Randevular
                .Include(r => r.Antrenor)
                .GroupBy(r => new { r.AntrenorId, r.Antrenor.Ad, r.Antrenor.Soyad })
                .Select(g => new
                {
                    AntrenorId = g.Key.AntrenorId,
                    AntrenorAdSoyad = g.Key.Ad + " " + g.Key.Soyad,
                    RandevuSayisi = g.Count()
                })
                .OrderByDescending(x => x.RandevuSayisi)
                .ToListAsync();

            // يرجّع JSON
            return Ok(sonuc);
        }

        // ----------------------------------------------------
        // 2) Belirli bir üyenin tüm randevuları
        // GET: /api/rapor/uye-randevular/5
        // ----------------------------------------------------
        [HttpGet("uye-randevular/{uyeId:int}")]
        public async Task<IActionResult> UyeRandevular(int uyeId)
        {
            var randevular = await _context.Randevular
                .Where(r => r.UyeId == uyeId)
                .Include(r => r.Antrenor)
                .Include(r => r.Salon)
                .OrderBy(r => r.TarihSaat)
                .Select(r => new
                {
                    r.Id,
                    r.TarihSaat,
                    r.Not,
                    Antrenor = r.Antrenor.Ad + " " + r.Antrenor.Soyad,
                    Salon = r.Salon.Ad
                })
                .ToListAsync();

            return Ok(randevular); // JSON
        }
    }
}
