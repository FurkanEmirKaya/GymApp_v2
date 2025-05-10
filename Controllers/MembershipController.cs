using System.Security.Claims;
using GymApp_v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace efCoreApp.AddControllers
{
    [AllowAnonymous]
    public class MembershipController : Controller
    {
        private readonly DataContext _context;

        public MembershipController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllMemberships()
        {
            var memberships = await _context.Memberships.ToListAsync();
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            
            var subscription = await _context.Subscriptions
                .Where(s => s.User.Email == userEmail && s.EndDate > DateTime.Now)
                .FirstOrDefaultAsync(); 
            
            ViewBag.Subscription = subscription;
            
            return View(memberships);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
                return NotFound();

            return View(membership); // Views/Membership/Details.cshtml olacak
        }

       

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
                return NotFound();

            _context.Memberships.Remove(membership);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewAllMemberships");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
                return NotFound();

            return View(membership);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Membership updatedMembership, IFormFile? newImage)
        {
            var membership = await _context.Memberships.FindAsync(updatedMembership.Id);
            if (membership == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(updatedMembership);

            // Alanları güncelle
            membership.Title = updatedMembership.Title;
            membership.Description = updatedMembership.Description;
            membership.DurationInDays = updatedMembership.DurationInDays;
            membership.Price = updatedMembership.Price;
            membership.Type = updatedMembership.Type;

            if (newImage != null && newImage.Length > 0)
            {
                using var ms = new MemoryStream();
                await newImage.CopyToAsync(ms);
                membership.Image = ms.ToArray();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewAllMemberships");
        }











    }
}
