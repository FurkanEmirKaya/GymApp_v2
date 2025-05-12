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
            // Üyelik var mı kontrol et
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
                return NotFound("Üyelik bulunamadı.");

            // Giriş yapan kullanıcıyı al
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return Unauthorized();

            // Kullanıcının aktif bir üyeliği var mı kontrol et
            var existingActiveSubscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.EndDate > DateTime.Now);

            if (existingActiveSubscription != null)
            {
                TempData["Info"] = "Zaten aktif bir üyeliğiniz var. Yeni üyelik satın almak için mevcut üyeliğinizin bitmesini bekleyebilir veya iptal edebilirisiniz.";
                return RedirectToAction("Dashboard", "Profile");
            }

            // Yeni abonelik oluştur
            var subscription = new Subscription
            {
                UserId = user.Id,
                MembershipId = membership.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(membership.DurationInDays)
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            TempData["Info"] = "Üyelik başarıyla satın alındı.";
            return RedirectToAction("Dashboard", "Profile");
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return Unauthorized();

            // İlgili aboneliği getir
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id && s.EndDate > DateTime.Now);

            if (subscription == null)
            {
                TempData["Error"] = "Aktif aboneliğiniz bulunamadı veya zaten sona ermiş.";
                return RedirectToAction("Dashboard", "Profile");
            }

            // Soft-cancel: Aslında veri tabanından silinmez. İlerisi için kayıt olur. (Bugün itibariyle bitir)
            subscription.EndDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Info"] = "Aboneliğiniz iptal edildi.";
            return RedirectToAction("DashboardCancelledMembership", "Profile");
        }
    }
}
