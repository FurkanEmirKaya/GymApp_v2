using Microsoft.AspNetCore.Mvc;
using GymApp_v1.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using GymApp_v1.Models;

namespace GymApp_v1.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly DataContext _context;

        public AuthenticationController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View(); // bu Register.cshtml'i döndürür
        }


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // ✅ Giriş başarılıysa role göre yönlendirme
                if (user.Role == "Admin")
                    return RedirectToAction("ListAllUsers", "Admin");
                else
                    return RedirectToAction("Dashboard", "Profile");
            }

            ViewBag.Error = "Geçersiz email veya şifre";
            return View();
        }



        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userExists = _context.Users.Any(u => u.Email == model.Email);
                if (userExists)
                {
                    ModelState.AddModelError("", "Bu e-posta ile zaten kayıt olunmuş.");
                    return View(model);
                }

                var newUser = new User
                {
                    Username = model.Email.Split('@')[0],
                    Email = model.Email,
                    Password = model.Password,
                    Role = "User"
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(); // ✅ BURASI DB'ye kaydeder

                // Otomatik giriş için cookie oluştur
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, newUser.Username),
            new Claim(ClaimTypes.Email, newUser.Email),
            new Claim(ClaimTypes.Role, newUser.Role)
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Login", "Authentication");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Bu e-posta sistemde bulunamadı.";
                return View();
            }

            // Email doğruysa reset sayfasına yönlendir
            return View("ResetPassword", new ResetPasswordViewModel { Email = email });
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ViewBag.Error = "Kullanıcı bulunamadı.";
                return View(model);
            }

            user.Password = model.NewPassword;
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Authentication");
        }

    }
}
