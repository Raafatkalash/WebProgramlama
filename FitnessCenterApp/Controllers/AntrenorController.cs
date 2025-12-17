using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;

namespace FitnessCenterApp.Controllers
{
    public class AntrenorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Antrenor
        public async Task<IActionResult> Index()
        {
            var list = await _context.Antrenorler.ToListAsync();
            return View(list);
        }

        // GET: /Antrenor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Antrenor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Antrenor antrenor)
        {
            if (!ModelState.IsValid)
            {
                return View(antrenor);
            }

            _context.Add(antrenor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // (بعدين منعمل Edit / Details / Delete بنفس النمط)
    }
}
