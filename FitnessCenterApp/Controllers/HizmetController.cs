using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HizmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Hizmetler
                .Include(h => h.Salon)
                .OrderBy(h => h.Ad)
                .ToListAsync();

            return View(list);
        }

        public IActionResult Create()
        {
            ViewData["SalonId"] = new SelectList(_context.Salonlar.OrderBy(s => s.Ad), "Id", "Ad");
            return View();
        }

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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            ViewData["SalonId"] = new SelectList(_context.Salonlar.OrderBy(s => s.Ad), "Id", "Ad", hizmet.SalonId);
            return View(hizmet);
        }

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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler
                .Include(h => h.Antrenorler)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hizmet == null) return RedirectToAction(nameof(Index));

            var ilgiliRandevular = await _context.Randevular
                .Where(r => r.HizmetId == id)
                .ToListAsync();
            _context.Randevular.RemoveRange(ilgiliRandevular);

            hizmet.Antrenorler.Clear();
            _context.Hizmetler.Remove(hizmet);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
