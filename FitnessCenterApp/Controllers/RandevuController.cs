using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;

namespace FitnessCenterApp.Controllers
{
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Randevu
        public async Task<IActionResult> Index()
        {
            var randevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Salon)
                .OrderBy(r => r.TarihSaat)
                .ToListAsync();

            return View(randevular);
        }

        // دالة صغيرة لإعادة تعبئة الـ dropdowns
        private void LoadDropdowns(int? selectedUyeId = null,
                                   int? selectedAntrenorId = null,
                                   int? selectedSalonId = null)
        {
            // لو ما عندك AdSoyad استخدم Ad فقط
            ViewData["UyeId"] = new SelectList(
                _context.Uyeler.OrderBy(u => u.Ad),
                "Id",
                "Ad",
                selectedUyeId
            );

            ViewData["AntrenorId"] = new SelectList(
                _context.Antrenorler.OrderBy(a => a.Ad),
                "Id",
                "Ad",
                selectedAntrenorId
            );

            ViewData["SalonId"] = new SelectList(
                _context.Salonlar.OrderBy(s => s.Ad),
                "Id",
                "Ad",
                selectedSalonId
            );
        }

        // GET: /Randevu/Create
        public IActionResult Create()
        {
            LoadDropdowns();

            // قيمة افتراضية للتاريخ (مثلاً بعد ساعة من الآن)
            var model = new Randevu
            {
                TarihSaat = DateTime.Now.AddHours(1)
            };

            return View(model);
        }

        // POST: /Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            if (!ModelState.IsValid)
            {
                // لو في خطأ نرجع نفس الصفحة مع الأخطاء و نعيد تعبئة الـ dropdowns
                LoadDropdowns(randevu.UyeId, randevu.AntrenorId, randevu.SalonId);
                return View(randevu);
            }

            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();

            // هون، إذا شفت حالك انتقلت إلى Index معناته الراندفو انضاف فعليًا
            return RedirectToAction(nameof(Index));
        }

        // GET: /Randevu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null)
                return NotFound();

            return View(randevu);
        }

        // POST: /Randevu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
