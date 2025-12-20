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
        // ✅ يعرض كل الأنترنور حتى لو 0 رانديفو
        [HttpGet("antrenor-randevu-sayisi")]
        public async Task<IActionResult> AntrenorRandevuSayisi()
        {
            var sonuc = await (
                from a in _context.Antrenorler.AsNoTracking()
                join r in _context.Randevular.AsNoTracking().Where(x => !x.IptalEdildi)
                    on a.Id equals r.AntrenorId into rg
                select new
                {
                    antrenorId = a.Id,
                    antrenorAdSoyad = a.Ad + " " + a.Soyad,
                    randevuSayisi = rg.Count()
                }
            )
            .OrderByDescending(x => x.randevuSayisi)
            .ThenBy(x => x.antrenorAdSoyad)
            .ToListAsync();

            return Ok(sonuc);
        }

        // GET: /api/rapor/uyeler
        // ✅ لتعرف الـ uyeId الحقيقي من جدول Uyeler
        [HttpGet("uyeler")]
        public async Task<IActionResult> Uyeler()
        {
            var uyeler = await _context.Uyeler
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    uyeId = u.Id,
                    adSoyad = u.Ad + " " + u.Soyad,
                    email = u.Email
                })
                .ToListAsync();

            return Ok(uyeler);
        }

        // GET: /api/rapor/uye-randevular/5
        // ✅ لازم تمرر UyeId الصحيح (مو شرط 1)
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
                    salon = r.Salon.Ad,
                    hizmet = r.Hizmet.Ad
                })
                .ToListAsync();

            return Ok(randevular);
        }
    }
}
