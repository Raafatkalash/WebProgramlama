using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    public class HizmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hizmet
        public async Task<IActionResult> Index()
        {
            var list = await _context.Hizmetler
                .Include(h => h.Salon)
                .OrderBy(h => h.Ad)
                .ToListAsync();

            return View(list);
        }

        // GET: Hizmet/Create
        public IActionResult Create()
        {
            ViewData["SalonId"] = new SelectList(_context.Salonlar.OrderBy(s => s.Ad), "Id", "Ad");
            return View();
        }

        // POST: Hizmet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hizmet hizmet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SalonId"] = new SelectList(_context.Salonlar.OrderBy(s => s.Ad), "Id", "Ad", hizmet.SalonId);
            return View(hizmet);
        }

        // GET: Hizmet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            ViewData["SalonId"] = new SelectList(_context.Salonlar.OrderBy(s => s.Ad), "Id", "Ad", hizmet.SalonId);
            return View(hizmet);
        }

        // POST: Hizmet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hizmet hizmet)
        {
            if (id != hizmet.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(hizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SalonId"] = new SelectList(_context.Salonlar.OrderBy(s => s.Ad), "Id", "Ad", hizmet.SalonId);
            return View(hizmet);
        }

        // GET: Hizmet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        // POST: Hizmet/Delete/5  ✅ حذف فعلي + حذف Randevular التابعة
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler
                .Include(h => h.Antrenorler)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hizmet == null) return RedirectToAction(nameof(Index));

            // 1) احذف كل randevular التابعة
            var ilgiliRandevular = await _context.Randevular
                .Where(r => r.HizmetId == id)
                .ToListAsync();
            _context.Randevular.RemoveRange(ilgiliRandevular);

            // 2) افصل علاقات many-to-many احتياطياً
            hizmet.Antrenorler.Clear();

            // 3) احذف الخدمة نفسها
            _context.Hizmetler.Remove(hizmet);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
