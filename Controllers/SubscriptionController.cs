using GymApp_v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace efCoreApp.AddControllers
{
    [Authorize(Roles = "User")]
    public class SubscriptionController : Controller
    {
        private readonly DataContext _context;

        public SubscriptionController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Buy(int id)
        {
            // �yelik var m� kontrol et
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
                return NotFound("�yelik bulunamad�.");

            // Giri� yapan kullan�c�y� al
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return Unauthorized();

            // Kullan�c�n�n aktif bir �yeli�i var m� kontrol et
            var existingActiveSubscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.EndDate > DateTime.Now);

            if (existingActiveSubscription != null)
            {
                TempData["Info"] = "Zaten aktif bir �yeli�iniz var. Yeni �yelik sat�n almak i�in mevcut �yeli�inizin bitmesini bekleyin.";
                return RedirectToAction("Dashboard", "Profile");
            }

            // Yeni abonelik olu�tur
            var subscription = new Subscription
            {
                UserId = user.Id,
                MembershipId = membership.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(membership.DurationInDays)
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            TempData["Info"] = "�yelik ba�ar�yla sat�n al�nd�.";
            return RedirectToAction("Dashboard", "Profile");
        }
    }
}
