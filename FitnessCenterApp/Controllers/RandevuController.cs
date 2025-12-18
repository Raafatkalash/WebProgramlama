using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void FillDropdowns(Randevu? r = null)
        {
            ViewData["UyeId"] = new SelectList(_context.Uyeler, "Id", "AdSoyad", r?.UyeId);
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", r?.AntrenorId);
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", r?.SalonId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", r?.HizmetId);
        }

        // GET: Randevu
        public async Task<IActionResult> Index()
        {
            var randevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Salon)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.TarihSaat)
                .ToListAsync();

            return View(randevular);
        }

        // GET: Randevu/Create
        public IActionResult Create()
        {
            FillDropdowns();
            return View(new Randevu { TarihSaat = DateTime.Now.AddMinutes(30) });
        }

        // POST: Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            // 0) لو التاريخ ما وصل (default) => مشكلة format
            if (randevu.TarihSaat == default)
                ModelState.AddModelError("TarihSaat", "Tarih seçilmedi veya format hatalı.");

            // 1) لا يسمح بالماضي (مع هامش 1 دقيقة)
            if (randevu.TarihSaat <= DateTime.Now.AddMinutes(1))
                ModelState.AddModelError("TarihSaat", "Geçmiş bir zaman için randevu alınamaz.");

            // 2) جلب الخدمة + الصالة + المدرب
            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(h => h.Id == randevu.HizmetId);

            if (hizmet == null)
                ModelState.AddModelError("HizmetId", "Seçilen hizmet bulunamadı.");

            var antrenor = await _context.Antrenorler
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.Id == randevu.AntrenorId);

            if (antrenor == null)
                ModelState.AddModelError("AntrenorId", "Seçilen antrenör bulunamadı.");

            var salon = await _context.Salonlar
                .FirstOrDefaultAsync(s => s.Id == randevu.SalonId);

            if (salon == null)
                ModelState.AddModelError("SalonId", "Seçilen salon bulunamadı.");

            if (hizmet != null && antrenor != null && salon != null)
            {
                // 3) الخدمة لازم تكون لنفس الصالة المختارة
                if (hizmet.SalonId != randevu.SalonId)
                    ModelState.AddModelError("SalonId", "Seçilen hizmet bu salona ait değil.");

                // 4) المدرب يقدم هذه الخدمة
                if (!antrenor.Hizmetler.Any(h => h.Id == randevu.HizmetId))
                    ModelState.AddModelError("HizmetId", "Bu antrenör seçilen hizmeti vermiyor.");

                // 5) احسب نهاية الموعد من مدة الخدمة
                randevu.BitisTarihSaat = randevu.TarihSaat.AddMinutes(hizmet.SureDakika);

                var bas = randevu.TarihSaat.TimeOfDay;
                var bit = randevu.BitisTarihSaat.TimeOfDay;

                // 6) ساعات عمل الصالة
                if (bas < salon.AcilisSaati || bit > salon.KapanisSaati)
                    ModelState.AddModelError("TarihSaat", "Randevu salon çalışma saatleri dışında olamaz.");

                // 7) توفر المدرب
                if (bas < antrenor.MusaitBaslangic || bit > antrenor.MusaitBitis)
                    ModelState.AddModelError("TarihSaat", "Randevu antrenörün uygun saatleri dışında olamaz.");

                // 8) تعارض زمني للمدرب
                bool cakisanAntrenor = await _context.Randevular.AnyAsync(r =>
                    !r.IptalEdildi &&
                    r.AntrenorId == randevu.AntrenorId &&
                    r.TarihSaat < randevu.BitisTarihSaat &&
                    randevu.TarihSaat < r.BitisTarihSaat);

                if (cakisanAntrenor)
                    ModelState.AddModelError("TarihSaat", "Bu saat aralığında bu antrenörde zaten bir randevu var.");

                // 9) تعارض زمني للعضو
                bool cakisanUye = await _context.Randevular.AnyAsync(r =>
                    !r.IptalEdildi &&
                    r.UyeId == randevu.UyeId &&
                    r.TarihSaat < randevu.BitisTarihSaat &&
                    randevu.TarihSaat < r.BitisTarihSaat);

                if (cakisanUye)
                    ModelState.AddModelError("TarihSaat", "Bu saat aralığında üyeye ait başka bir randevu var.");

                if (ModelState.IsValid)
                {
                    // Snapshot
                    randevu.HizmetAdi = hizmet.Ad;
                    randevu.HizmetSureDakika = hizmet.SureDakika;
                    randevu.HizmetUcret = hizmet.Ucret;

                    randevu.Onayli = false;
                    randevu.IptalEdildi = false;

                    _context.Add(randevu);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            FillDropdowns(randevu);
            return View(randevu);
        }

        // GET: Randevu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            if (randevu.IptalEdildi) return RedirectToAction(nameof(Index));

            FillDropdowns(randevu);
            return View(randevu);
        }

        // POST: Randevu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Randevu randevu)
        {
            if (id != randevu.Id) return NotFound();

            var mevcut = await _context.Randevular.FirstOrDefaultAsync(r => r.Id == id);
            if (mevcut == null) return NotFound();
            if (mevcut.IptalEdildi) return RedirectToAction(nameof(Index));

            if (randevu.TarihSaat == default)
                ModelState.AddModelError("TarihSaat", "Tarih seçilmedi veya format hatalı.");

            if (randevu.TarihSaat <= DateTime.Now.AddMinutes(1))
                ModelState.AddModelError("TarihSaat", "Geçmiş bir zaman için randevu alınamaz.");

            var hizmet = await _context.Hizmetler.Include(h => h.Salon).FirstOrDefaultAsync(h => h.Id == randevu.HizmetId);
            var antrenor = await _context.Antrenorler.Include(a => a.Hizmetler).FirstOrDefaultAsync(a => a.Id == randevu.AntrenorId);
            var salon = await _context.Salonlar.FirstOrDefaultAsync(s => s.Id == randevu.SalonId);

            if (hizmet == null) ModelState.AddModelError("HizmetId", "Seçilen hizmet bulunamadı.");
            if (antrenor == null) ModelState.AddModelError("AntrenorId", "Seçilen antrenör bulunamadı.");
            if (salon == null) ModelState.AddModelError("SalonId", "Seçilen salon bulunamadı.");

            if (hizmet != null && antrenor != null && salon != null)
            {
                if (hizmet.SalonId != randevu.SalonId)
                    ModelState.AddModelError("SalonId", "Seçilen hizmet bu salona ait değil.");

                if (!antrenor.Hizmetler.Any(h => h.Id == randevu.HizmetId))
                    ModelState.AddModelError("HizmetId", "Bu antrenör seçilen hizmeti vermiyor.");

                randevu.BitisTarihSaat = randevu.TarihSaat.AddMinutes(hizmet.SureDakika);

                var bas = randevu.TarihSaat.TimeOfDay;
                var bit = randevu.BitisTarihSaat.TimeOfDay;

                if (bas < salon.AcilisSaati || bit > salon.KapanisSaati)
                    ModelState.AddModelError("TarihSaat", "Randevu salon çalışma saatleri dışında olamaz.");

                if (bas < antrenor.MusaitBaslangic || bit > antrenor.MusaitBitis)
                    ModelState.AddModelError("TarihSaat", "Randevu antrenörün uygun saatleri dışında olamaz.");

                bool cakisanAntrenor = await _context.Randevular.AnyAsync(r =>
                    !r.IptalEdildi &&
                    r.Id != id &&
                    r.AntrenorId == randevu.AntrenorId &&
                    r.TarihSaat < randevu.BitisTarihSaat &&
                    randevu.TarihSaat < r.BitisTarihSaat);

                if (cakisanAntrenor)
                    ModelState.AddModelError("TarihSaat", "Bu saat aralığında bu antrenörde zaten bir randevu var.");

                bool cakisanUye = await _context.Randevular.AnyAsync(r =>
                    !r.IptalEdildi &&
                    r.Id != id &&
                    r.UyeId == randevu.UyeId &&
                    r.TarihSaat < randevu.BitisTarihSaat &&
                    randevu.TarihSaat < r.BitisTarihSaat);

                if (cakisanUye)
                    ModelState.AddModelError("TarihSaat", "Bu saat aralığında üyeye ait başka bir randevu var.");

                if (ModelState.IsValid)
                {
                    // حافظ على onay/iptal
                    randevu.Onayli = mevcut.Onayli;
                    randevu.IptalEdildi = mevcut.IptalEdildi;

                    // Snapshot
                    randevu.HizmetAdi = hizmet.Ad;
                    randevu.HizmetSureDakika = hizmet.SureDakika;
                    randevu.HizmetUcret = hizmet.Ucret;

                    _context.Entry(mevcut).CurrentValues.SetValues(randevu);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            FillDropdowns(randevu);
            return View(randevu);
        }

        // POST: Randevu/Delete/5  (إلغاء بدل حذف)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.IptalEdildi = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Randevu/Onayla/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            if (!randevu.IptalEdildi)
            {
                randevu.Onayli = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
