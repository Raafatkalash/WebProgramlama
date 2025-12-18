using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            var antrenorler = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.Hizmetler)
                .ToListAsync();

            return View(antrenorler);
        }

        // GET: Antrenor/Create
        public IActionResult Create()
        {
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad");
            ViewData["Hizmetler"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad");
            return View();
        }

        // POST: Antrenor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Antrenor antrenor,
            int[] selectedHizmetler,     // لو View اسمها selectedHizmetler
            int[] Hizmetler              // لو View اسمها Hizmetler
        )
        {
            // ✅ دعم الاسمين: selectedHizmetler أو Hizmetler
            var secilenIds = (selectedHizmetler != null && selectedHizmetler.Length > 0)
                ? selectedHizmetler
                : (Hizmetler ?? Array.Empty<int>());

            // ✅ salon لازم
            if (antrenor.SalonId == 0)
                ModelState.AddModelError("SalonId", "Salon seçiniz.");

            if (!ModelState.IsValid)
            {
                ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", antrenor.SalonId);
                ViewData["Hizmetler"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", secilenIds);
                return View(antrenor);
            }

            // ✅ احفظ علاقات الخدمات
            antrenor.Hizmetler = await _context.Hizmetler
                .Where(h => secilenIds.Contains(h.Id))
                .ToListAsync();

            _context.Antrenorler.Add(antrenor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Antrenor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", antrenor.SalonId);
            ViewData["Hizmetler"] = new MultiSelectList(
                _context.Hizmetler,
                "Id",
                "Ad",
                antrenor.Hizmetler.Select(h => h.Id).ToArray()
            );

            return View(antrenor);
        }

        // POST: Antrenor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Antrenor antrenor,
            int[] selectedHizmetler,     // لو View اسمها selectedHizmetler
            int[] Hizmetler              // لو View اسمها Hizmetler
        )
        {
            if (id != antrenor.Id) return NotFound();

            var mevcut = await _context.Antrenorler
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (mevcut == null) return NotFound();

            var secilenIds = (selectedHizmetler != null && selectedHizmetler.Length > 0)
                ? selectedHizmetler
                : (Hizmetler ?? Array.Empty<int>());

            if (antrenor.SalonId == 0)
                ModelState.AddModelError("SalonId", "Salon seçiniz.");

            if (!ModelState.IsValid)
            {
                ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", antrenor.SalonId);
                ViewData["Hizmetler"] = new MultiSelectList(_context.Hizmetler, "Id", "Ad", secilenIds);
                return View(antrenor);
            }

            // ✅ تحديث الحقول العادية
            mevcut.Ad = antrenor.Ad;
            mevcut.Soyad = antrenor.Soyad;
            mevcut.UzmanlikAlani = antrenor.UzmanlikAlani;
            mevcut.Telefon = antrenor.Telefon;
            mevcut.Email = antrenor.Email;
            mevcut.SalonId = antrenor.SalonId;
            mevcut.MusaitBaslangic = antrenor.MusaitBaslangic;
            mevcut.MusaitBitis = antrenor.MusaitBitis;

            // ✅ تحديث علاقة الخدمات (many-to-many)
            mevcut.Hizmetler.Clear();
            var yeniHizmetler = await _context.Hizmetler
                .Where(h => secilenIds.Contains(h.Id))
                .ToListAsync();
            foreach (var h in yeniHizmetler)
                mevcut.Hizmetler.Add(h);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Antrenor/Delete/5  (صفحة تأكيد)
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

        // POST: Antrenor/Delete/5  (حذف فعلي)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return RedirectToAction(nameof(Index));

            try
            {
                // ✅ إذا في رandevu مرتبط بالمدرب: نحذفهم فعلياً لتجنب FK error
                var randevular = await _context.Randevular
                    .Where(r => r.AntrenorId == id)
                    .ToListAsync();
                if (randevular.Count > 0)
                    _context.Randevular.RemoveRange(randevular);

                // ✅ امسح الربط مع الخدمات
                antrenor.Hizmetler.Clear();

                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Antrenör başarıyla silindi.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Bu kayıt silinemedi (ilişkili kayıtlar var).";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
