using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RandevuController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // Helpers
        // =========================

        private async Task<Uye?> GetCurrentUyeAsync()
        {
            var email = User?.Identity?.Name; // غالباً = Email
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // Role kontrolü (Admin değilse Uye olsun)
            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser != null)
            {
                bool isAdmin = await _userManager.IsInRoleAsync(identityUser, "Admin");
                bool isUye = await _userManager.IsInRoleAsync(identityUser, "Uye");

                if (!isAdmin && !isUye)
                    await _userManager.AddToRoleAsync(identityUser, "Uye");
            }

            // فقط نبحث.. إنشاء Uye يكون في Register
            return await _context.Uyeler.FirstOrDefaultAsync(u => u.Email == email);
        }

        private IActionResult RedirectToRegisterWithError()
        {
            TempData["Error"] = "Üye kaydınız bulunamadı. Lütfen kayıt olun (Register).";
            return RedirectToAction("Register", "Account");
        }

        private void FillDropdownsForAdmin(Randevu? r = null)
        {
            ViewData["UyeId"] = new SelectList(_context.Uyeler, "Id", "AdSoyad", r?.UyeId);
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", r?.AntrenorId);
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", r?.SalonId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", r?.HizmetId);
        }

        private void FillDropdownsForUye(Uye uye, Randevu? r = null)
        {
            ViewData["UyeId"] = new SelectList(new[] { uye }, "Id", "AdSoyad", uye.Id);
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", r?.AntrenorId);
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", r?.SalonId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", r?.HizmetId);
        }

        // =========================
        // GET: Randevu
        // Admin: كل الرانديفوهات
        // Uye: فقط رانديفوهاته
        // =========================
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var all = await _context.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Salon)
                    .Include(r => r.Hizmet)
                    .OrderByDescending(r => r.TarihSaat)
                    .ToListAsync();

                return View(all);
            }

            // Uye
            var uye = await GetCurrentUyeAsync();
            if (uye == null)
                return RedirectToRegisterWithError();

            var list = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Salon)
                .Include(r => r.Hizmet)
                .Where(r => r.UyeId == uye.Id)
                .OrderByDescending(r => r.TarihSaat)
                .ToListAsync();

            return View(list);
        }

        // =========================
        // GET: Randevu/Create
        // =========================
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
            {
                FillDropdownsForAdmin();
                return View(new Randevu { TarihSaat = DateTime.Now.AddMinutes(30) });
            }

            var uye = await GetCurrentUyeAsync();
            if (uye == null)
                return RedirectToRegisterWithError();

            var r = new Randevu
            {
                UyeId = uye.Id,
                TarihSaat = DateTime.Now.AddMinutes(30)
            };

            FillDropdownsForUye(uye, r);
            return View(r);
        }

        // =========================
        // POST: Randevu/Create
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            // لو Uye: نفرض UyeId الصحيح
            Uye? currentUye = null;
            if (!User.IsInRole("Admin"))
            {
                currentUye = await GetCurrentUyeAsync();
                if (currentUye == null)
                    return RedirectToRegisterWithError();

                randevu.UyeId = currentUye.Id;
            }

            // ========= VALIDATIONS =========

            if (randevu.TarihSaat == default)
                ModelState.AddModelError("TarihSaat", "Tarih seçilmedi veya format hatalı.");

            if (randevu.TarihSaat <= DateTime.Now.AddMinutes(1))
                ModelState.AddModelError("TarihSaat", "Geçmiş bir zaman için randevu alınamaz.");

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
                // 1) Hizmet aynı salonun olmalı
                if (hizmet.SalonId != randevu.SalonId)
                    ModelState.AddModelError("SalonId", "Seçilen hizmet bu salona ait değil.");

                // 2) Antrenör bu hizmeti vermeli
                if (!antrenor.Hizmetler.Any(h => h.Id == randevu.HizmetId))
                    ModelState.AddModelError("HizmetId", "Bu antrenör seçilen hizmeti vermiyor.");

                // 3) Bitiş zamanı = başlangıç + süre
                randevu.BitisTarihSaat = randevu.TarihSaat.AddMinutes(hizmet.SureDakika);

                var bas = randevu.TarihSaat.TimeOfDay;
                var bit = randevu.BitisTarihSaat.TimeOfDay;

                // 4) Salon çalışma saatleri
                if (bas < salon.AcilisSaati || bit > salon.KapanisSaati)
                    ModelState.AddModelError("TarihSaat", "Randevu salon çalışma saatleri dışında olamaz.");

                // 5) Antrenör müsaitliği
                if (bas < antrenor.MusaitBaslangic || bit > antrenor.MusaitBitis)
                    ModelState.AddModelError("TarihSaat", "Randevu antrenörün uygun saatleri dışında olamaz.");

                // 6) Çakışma - Antrenör
                bool cakisanAntrenor = await _context.Randevular.AnyAsync(r =>
                    !r.IptalEdildi &&
                    r.AntrenorId == randevu.AntrenorId &&
                    r.TarihSaat < randevu.BitisTarihSaat &&
                    randevu.TarihSaat < r.BitisTarihSaat);

                if (cakisanAntrenor)
                    ModelState.AddModelError("TarihSaat", "Bu saat aralığında bu antrenörde zaten bir randevu var.");

                // 7) Çakışma - Üye
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
                    TempData["Success"] = "Randevu oluşturuldu. Onay bekliyor.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // dropdowns
            if (User.IsInRole("Admin"))
                FillDropdownsForAdmin(randevu);
            else
                FillDropdownsForUye(currentUye!, randevu);

            return View(randevu);
        }

        // =========================
        // POST: Randevu/Delete (İptal)
        // Admin: أي رانديفو
        // Uye: فقط رانديفوه
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return RedirectToAction(nameof(Index));

            if (!User.IsInRole("Admin"))
            {
                var uye = await GetCurrentUyeAsync();
                if (uye == null) return RedirectToRegisterWithError();

                if (randevu.UyeId != uye.Id)
                    return Forbid();
            }

            randevu.IptalEdildi = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // POST: Randevu/Onayla (Admin فقط)
        // =========================
        [Authorize(Roles = "Admin")]
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
                TempData["Success"] = "Randevu onaylandı.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
