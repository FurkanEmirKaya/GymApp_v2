using Microsoft.AspNetCore.Mvc;
using GymApp_v1.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using GymApp_v1.Models;
using Microsoft.AspNetCore.Identity;
using GymApp_v1.ViewModels;
using GymApp_v2.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GymApp_v1.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly DataContext _context;
        private readonly IPasswordHasher<User> _hasher;
            private readonly IEmailService _emailService;
    public AuthenticationController(DataContext context, IPasswordHasher<User> hasher, IEmailService emailService)
    {
        _context = context;
        _hasher = hasher;
        _emailService = emailService;
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            
            if (user == null)
            {
                ModelState.AddModelError("Email", "Bu email adresi sistemde kayıtlı değil.");
                return View(model);
            }
            
            var result = _hasher.VerifyHashedPassword(user, user.Password, model.Password);
            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe, // Beni hatırla seçeneği
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 30 : 1)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                if (user.Role == "Admin")
                {
                    return Redirect("/Admin/ListAllUsers");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Profile");
                }
            }

            ModelState.AddModelError("Password", "Şifre yanlış.");
            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

       [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            
            // Email kontrolü
            var userExists = _context.Users.Any(u => u.Email == model.Email);
            if (userExists)
            {
                ModelState.AddModelError("Email", "Bu e-posta ile zaten kayıt olunmuş.");
                return View(model);
            }
            
            var newUser = new User
            {
                Username = model.Email.Split('@')[0],
                Email = model.Email,
                Role = "User",
                Password = string.Empty // Geçici değer
            };

            // Şifreyi hash'le
            newUser.Password = _hasher.HashPassword(newUser, model.Password);
            
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Otomatik giriş için cookie oluştur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, newUser.Username),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.Role, newUser.Role),
                new Claim("UserId", newUser.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Başarılı kayıt sonrası dashboard'a yönlendir
            return RedirectToAction("Dashboard", "Profile");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                // Güvenlik için kullanıcıya email bulunamadığını söylemeyin
                // Başarılı mesajı gösterin
                TempData["SuccessMessage"] = "Eğer bu email adresi sistemde kayıtlı ise, şifre sıfırlama bağlantısı gönderilecektir.";
                return RedirectToAction("Login");
            }

            // Token oluştur
            var token = GeneratePasswordResetToken();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(24); // 24 saat geçerli

            await _context.SaveChangesAsync();

            // Reset bağlantısı oluştur - null reference kontrolü ekleyin
            var resetLink = Url.Action("ResetPassword", "Authentication", 
                new { token = token }, Request.Scheme);
            
            if (string.IsNullOrEmpty(resetLink))
            {
                TempData["ErrorMessage"] = "Şifre sıfırlama bağlantısı oluşturulamadı.";
                return RedirectToAction("Login");
            }

            // Email gönder
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
                TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı email adresinize gönderildi.";
            }
            catch (Exception ex)
            {
                // Log the error - ex kullanımı için
                Console.WriteLine($"Email gönderimi hatası: {ex.Message}");
                // veya logger kullanın:
                // _logger.LogError(ex, "Email gönderimi sırasında hata oluştu");
                
                TempData["ErrorMessage"] = "Email gönderimi sırasında bir hata oluştu. Lütfen tekrar deneyin.";
            }

            return RedirectToAction("Login");
        }


    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.PasswordResetToken == token && 
            u.PasswordResetTokenExpires > DateTime.UtcNow);

        if (user == null)
        {
            TempData["ErrorMessage"] = "Geçersiz veya süresi dolmuş token.";
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordViewModel
        {
            Email = user.Email,
            NewPassword = string.Empty,  // required property'yi set edin
            ConfirmPassword = string.Empty,  // required property'yi set edin
            Token = token  // Token'ı set edin
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.PasswordResetToken == model.Token && 
            u.PasswordResetTokenExpires > DateTime.UtcNow);

        if (user == null)
        {
            ModelState.AddModelError("", "Geçersiz veya süresi dolmuş token.");
            return View(model);
        }

        user.Password = _hasher.HashPassword(user, model.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
        return RedirectToAction("Login");
    }

    private string GeneratePasswordResetToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
}
