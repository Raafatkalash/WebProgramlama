using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class RaporController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RaporController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/rapor/antrenor-randevu-sayisi
        // ✅ LINQ + REST API + DB
        [HttpGet("antrenor-randevu-sayisi")]
        public async Task<IActionResult> AntrenorRandevuSayisi()
        {
            var sonuc = await _context.Randevular
                .AsNoTracking()
                .Where(r => !r.IptalEdildi)
                .GroupBy(r => new { r.AntrenorId, r.Antrenor.Ad, r.Antrenor.Soyad })
                .Select(g => new
                {
                    antrenorId = g.Key.AntrenorId,
                    antrenorAdSoyad = g.Key.Ad + " " + g.Key.Soyad,
                    randevuSayisi = g.Count()
                })
                .OrderByDescending(x => x.randevuSayisi)
                .ToListAsync();

            return Ok(sonuc);
        }

        // GET: /api/rapor/uye-randevular/1
        // ✅ LINQ filtering (uyeId)
        [HttpGet("uye-randevular/{uyeId:int}")]
        public async Task<IActionResult> UyeRandevular(int uyeId)
        {
            var randevular = await _context.Randevular
                .AsNoTracking()
                .Where(r => r.UyeId == uyeId && !r.IptalEdildi)
                .OrderBy(r => r.TarihSaat)
                .Select(r => new
                {
                    id = r.Id,
                    tarihSaat = r.TarihSaat,
                    not = r.Not,
                    antrenor = r.Antrenor.Ad + " " + r.Antrenor.Soyad,
                    salon = r.Salon.Ad
                })
                .ToListAsync();

            return Ok(randevular);
        }
    }
}
