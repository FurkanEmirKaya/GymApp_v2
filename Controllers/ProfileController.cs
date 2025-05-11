using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymApp_v1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GymApp_v1.ViewModels;

namespace GymApp_v1.Controllers
{
    [Authorize(Roles = "User")] // sadece "User" rol�ndeki kullan�c�lar g�rebilir
    public class ProfileController : Controller
    {
        private readonly DataContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        public ProfileController(DataContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        
        public async Task<IActionResult> Dashboard()
        {   
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return Unauthorized();
        
            // Yalnızca aktif (EndDate > Now) aboneliği getir
            var subscription = await _context.Subscriptions
            .Include(s => s.Membership)
            .Where(s => s.UserId == user.Id && s.EndDate > DateTime.Now)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();
            
            ViewBag.Subscription = subscription;
            return View(user); // user yine Model
        }

                [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
                return Unauthorized();
            
            var viewModel = new EditProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CurrentProfilePicture = user.ProfilePicture
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
                return NotFound();
            
            // Kullanıcının sadece kendi profilini düzenleyebildiğinden emin olun
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (user.Email != currentUserEmail)
                return Forbid();
            
            // Email'in başka bir kullanıcı tarafından kullanılıp kullanılmadığını kontrol edin
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                return View(model);
            }
            
            // Verileri güncelle
            user.Email = model.Email;
            user.Username = model.Username;
            
            // Şifre güncellenecekse
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            }
            
            // Profil resmi güncellenecekse
            if (model.NewProfilePicture != null && model.NewProfilePicture.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.NewProfilePicture.CopyToAsync(ms);
                user.ProfilePicture = ms.ToArray();
            }
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction("Dashboard");
        }
        
        public IActionResult DashboardCancelledMembership()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return Unauthorized();
            return View(user); // user yine Model
        }


    }
}
