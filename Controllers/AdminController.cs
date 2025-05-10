using GymApp_v1.Data;
using GymApp_v1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace efCoreApp.AddControllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DataContext _context;

        public AdminController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult CreateMemberships()
        {
            ViewBag.TypeItems = new[]
            {
                new SelectListItem("Havuz Üyeliği", MembershipType.Pool.ToString()),
                new SelectListItem("Fitness Üyeliği", MembershipType.Fitness.ToString()),
                new SelectListItem("Gold Üyelik", MembershipType.Gold.ToString())
            };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMemberships(MembershipViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = new Membership
            {
                Title = model.Title,
                Description = model.Description,
                DurationInDays = model.DurationInDays,
                Type = model.Type.ToString(), // enum → string dönüşüm
                Price = model.Price,
                Image = model.Image != null ? FileToBytes(model.Image) : null
            };

            _context.Memberships.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("ListAllUsers", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> ListAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Subscriptions) // kullanıcıya ait abonelikleri dahil et
                    .ThenInclude(s => s.Membership) // aboneliğe ait üyelik bilgilerini dahil et
                .ToListAsync();

            return View(users);
        }


        private byte[] FileToBytes(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
