using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymApp_v1.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp_v1.Controllers
{
    [Authorize(Roles = "User")] // sadece "User" rol�ndeki kullan�c�lar g�rebilir
    public class ProfileController : Controller
    {
        private readonly DataContext _context;

        public ProfileController(DataContext context)
        {
            _context = context;
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

        public IActionResult ProfilePage()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return Unauthorized();
            return View(user); // user yine Model
        }

    }
}
