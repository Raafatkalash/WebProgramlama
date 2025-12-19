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

        [HttpGet("antrenor-randevu-sayisi")]
        public async Task<IActionResult> AntrenorRandevuSayisi()
        {
            var sonuc = await _context.Randevular
                .Where(r => !r.IptalEdildi)
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

            return Ok(sonuc);
        }

        [HttpGet("uye-randevular/{uyeId:int}")]
        public async Task<IActionResult> UyeRandevular(int uyeId)
        {
            var randevular = await _context.Randevular
                .Where(r => r.UyeId == uyeId && !r.IptalEdildi)
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

            return Ok(randevular);
        }
    }
}
