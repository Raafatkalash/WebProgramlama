using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RaporlamaController : Controller
    {
        [HttpGet("/Raporlama")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
