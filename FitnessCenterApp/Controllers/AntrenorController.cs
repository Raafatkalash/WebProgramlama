using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    public class AntrenorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Antrenor
        public async Task<IActionResult> Index()
        {
            var list = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.Hizmetler)
                .OrderBy(a => a.Ad)
                .ThenBy(a => a.Soyad)
                .ToListAsync();

            return View(list);
        }

        // GET: Antrenor/Create
        public async Task<IActionResult> Create()
        {
            await FillDropdownsForAntrenorAsync(selectedSalonId: null, selectedHizmetIds: null);
            return View();
        }

        // POST: Antrenor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Antrenor antrenor, int[] HizmetIds)
        {
            // Validations
            if (antrenor.SalonId <= 0)
                ModelState.AddModelError("SalonId", "Salon alanı zorunludur.");

            if (HizmetIds == null || HizmetIds.Length == 0)
                ModelState.AddModelError("HizmetIds", "En az 1 hizmet seçmelisiniz.");

            // Hizmetler salon ile uyumlu mu?
            if (ModelState.IsValid)
            {
                var secilenHizmetler = await _context.Hizmetler
                    .Where(h => HizmetIds.Contains(h.Id))
                    .ToListAsync();

                // Seçilen hizmetler boşsa
                if (secilenHizmetler.Count == 0)
                {
                    ModelState.AddModelError("HizmetIds", "Seçilen hizmetler bulunamadı.");
                }
                else
                {
                    // Salon uyumu kontrolü
                    bool salonUyumluDegil = secilenHizmetler.Any(h => h.SalonId != antrenor.SalonId);
                    if (salonUyumluDegil)
                        ModelState.AddModelError("HizmetIds", "Seçilen hizmetlerden bazıları bu salona ait değil.");

                    if (ModelState.IsValid)
                    {
                        antrenor.Hizmetler = secilenHizmetler;
                        _context.Antrenorler.Add(antrenor);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            await FillDropdownsForAntrenorAsync(antrenor.SalonId, HizmetIds);
            return View(antrenor);
        }

        // GET: Antrenor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            var selectedHizmetIds = antrenor.Hizmetler?.Select(h => h.Id).ToArray() ?? Array.Empty<int>();
            await FillDropdownsForAntrenorAsync(antrenor.SalonId, selectedHizmetIds);

            return View(antrenor);
        }

        // POST: Antrenor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Antrenor antrenor, int[] HizmetIds)
        {
            if (id != antrenor.Id) return NotFound();

            if (antrenor.SalonId <= 0)
                ModelState.AddModelError("SalonId", "Salon alanı zorunludur.");

            if (HizmetIds == null || HizmetIds.Length == 0)
                ModelState.AddModelError("HizmetIds", "En az 1 hizmet seçmelisiniz.");

            var mevcut = await _context.Antrenorler
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (mevcut == null) return NotFound();

            if (ModelState.IsValid)
            {
                var secilenHizmetler = await _context.Hizmetler
                    .Where(h => HizmetIds.Contains(h.Id))
                    .ToListAsync();

                if (secilenHizmetler.Count == 0)
                {
                    ModelState.AddModelError("HizmetIds", "Seçilen hizmetler bulunamadı.");
                }
                else
                {
                    bool salonUyumluDegil = secilenHizmetler.Any(h => h.SalonId != antrenor.SalonId);
                    if (salonUyumluDegil)
                        ModelState.AddModelError("HizmetIds", "Seçilen hizmetlerden bazıları bu salona ait değil.");

                    if (ModelState.IsValid)
                    {
                        // Scalar fields
                        mevcut.Ad = antrenor.Ad;
                        mevcut.Soyad = antrenor.Soyad;
                        mevcut.UzmanlikAlani = antrenor.UzmanlikAlani;
                        mevcut.Telefon = antrenor.Telefon;
                        mevcut.Email = antrenor.Email;
                        mevcut.SalonId = antrenor.SalonId;
                        mevcut.MusaitBaslangic = antrenor.MusaitBaslangic;
                        mevcut.MusaitBitis = antrenor.MusaitBitis;

                        // Update many-to-many
                        mevcut.Hizmetler.Clear();
                        foreach (var h in secilenHizmetler)
                            mevcut.Hizmetler.Add(h);

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            await FillDropdownsForAntrenorAsync(antrenor.SalonId, HizmetIds);
            return View(antrenor);
        }

        // GET: Antrenor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // POST: Antrenor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return RedirectToAction(nameof(Index));

            // إذا عندك Randevular مرتبطة بالمدرب، ممكن تمنع الحذف أو تحذفها/تلغيها حسب قرارك.
            bool hasRandevu = await _context.Randevular.AnyAsync(r => r.AntrenorId == id && !r.IptalEdildi);
            if (hasRandevu)
            {
                TempData["Error"] = "Bu antrenörün aktif randevuları olduğu için silinemez. Önce randevuları iptal ediniz.";
                return RedirectToAction(nameof(Index));
            }

            antrenor.Hizmetler.Clear();
            _context.Antrenorler.Remove(antrenor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helpers
        private async Task FillDropdownsForAntrenorAsync(int? selectedSalonId, int[]? selectedHizmetIds)
        {
            ViewBag.SalonId = new SelectList(
                await _context.Salonlar.OrderBy(s => s.Ad).ToListAsync(),
                "Id", "Ad",
                selectedSalonId
            );

            var hizmetlerQuery = _context.Hizmetler.AsQueryable();

            // إذا بدك تعرض فقط خدمات الصالة المختارة:
            if (selectedSalonId.HasValue && selectedSalonId.Value > 0)
                hizmetlerQuery = hizmetlerQuery.Where(h => h.SalonId == selectedSalonId.Value);

            var hizmetler = await hizmetlerQuery
                .OrderBy(h => h.Ad)
                .ToListAsync();

            var selectedSet = (selectedHizmetIds ?? Array.Empty<int>()).ToHashSet();

            ViewBag.HizmetList = hizmetler.Select(h => new SelectListItem
            {
                Value = h.Id.ToString(),
                Text = h.Ad,
                Selected = selectedSet.Contains(h.Id)
            }).ToList();
        }
    }
}
