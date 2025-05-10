using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymApp_v1.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp_v1.Controllers
{
    [Authorize(Roles = "User")] // sadece "User" rolündeki kullanýcýlar görebilir
    public class ProfileController : Controller
    {
        private readonly DataContext _context;

        public ProfileController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return Unauthorized();

            var subscription = _context.Subscriptions
                .Include(s => s.Membership)
                .FirstOrDefault(s => s.UserId == user.Id);

            ViewBag.Subscription = subscription;
            return View(user); // user yine Model
        }

    }
}
