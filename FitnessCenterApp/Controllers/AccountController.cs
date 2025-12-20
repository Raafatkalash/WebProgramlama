using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email ve şifre zorunludur.";
                return View();
            }

            // الأفضل: نجيب اليوزر بالإيميل ثم نسجل بـ UserName
            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user == null)
            {
                ViewBag.Error = "Giriş başarısız. Email veya şifre hatalı.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Giriş başarısız. Email veya şifre hatalı.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string ad, string soyad, string telefon, string email, string password)
        {
            ad = (ad ?? "").Trim();
            soyad = (soyad ?? "").Trim();
            telefon = (telefon ?? "").Trim();
            email = (email ?? "").Trim();

            if (string.IsNullOrWhiteSpace(ad))
            {
                ViewBag.Error = "Ad zorunludur.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(soyad))
            {
                ViewBag.Error = "Soyad zorunludur.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email ve şifre zorunludur.";
                return View();
            }

            // إذا في Üye بنفس الإيميل: ما نسمح
            bool uyeVarMi = await _context.Uyeler.AnyAsync(u => u.Email == email);
            if (uyeVarMi)
            {
                ViewBag.Error = "Bu email ile zaten Üye kaydı var. Lütfen giriş yapın.";
                return View();
            }

            // إذا في IdentityUser بنفس الإيميل: ما نسمح
            var existingIdentity = await _userManager.FindByEmailAsync(email);
            if (existingIdentity != null)
            {
                ViewBag.Error = "Bu email ile zaten kullanıcı var. Lütfen giriş yapın.";
                return View();
            }

            // ✅ Transaction: يا إمّا الاثنين معًا يا إمّا rollback
            using var trx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Create Identity User
                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var create = await _userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                {
                    await trx.RollbackAsync();
                    ViewBag.Error = "Kayıt başarısız: " + string.Join(", ", create.Errors.Select(e => e.Description));
                    return View();
                }

                // 2) Role = Uye
                await _userManager.AddToRoleAsync(user, "Uye");

                // 3) Create Uye row
                _context.Uyeler.Add(new Uye
                {
                    Ad = ad,
                    Soyad = soyad,
                    Telefon = telefon,
                    Email = email
                });
                await _context.SaveChangesAsync();

                await trx.CommitAsync();

                // 4) Sign in
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                await trx.RollbackAsync();
                ViewBag.Error = "Kayıt sırasında beklenmeyen hata oluştu.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
