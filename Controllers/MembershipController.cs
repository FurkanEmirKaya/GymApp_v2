using System.Security.Claims;
using GymApp_v1.Data;
using GymApp_v1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> ViewAllMemberships(string? searchTerm, string? typeFilter, string? priceFilter, string? sortBy)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            
            // Subscription kontrolü
            var subscription = await _context.Subscriptions
                .Where(s => s.User.Email == userEmail && s.EndDate > DateTime.Now)
                .FirstOrDefaultAsync();
            
            // Tüm memberships'leri başlangıçta al
            var membershipsQuery = _context.Memberships.AsQueryable();
            
            // Arama
            if (!string.IsNullOrEmpty(searchTerm))
            {
                membershipsQuery = membershipsQuery.Where(m => 
                    m.Title.Contains(searchTerm) || 
                    m.Description.Contains(searchTerm) ||
                    m.Type.Contains(searchTerm));
            }
            
            // Tip filtreleme
            if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "all")
            {
                membershipsQuery = membershipsQuery.Where(m => m.Type == typeFilter);
            }
            
            // Fiyat filtreleme
            if (!string.IsNullOrEmpty(priceFilter))
            {
                switch (priceFilter)
                {
                    case "low":
                        membershipsQuery = membershipsQuery.Where(m => m.Price < 1000);
                        break;
                    case "medium":
                        membershipsQuery = membershipsQuery.Where(m => m.Price >= 1000 && m.Price <= 3000);
                        break;
                    case "high":
                        membershipsQuery = membershipsQuery.Where(m => m.Price > 3000);
                        break;
                }
            }
            
            // Sıralama
            switch (sortBy)
            {
                case "price_asc":
                    membershipsQuery = membershipsQuery.OrderBy(m => m.Price);
                    break;
                case "price_desc":
                    membershipsQuery = membershipsQuery.OrderByDescending(m => m.Price);
                    break;
                case "duration_asc":
                    membershipsQuery = membershipsQuery.OrderBy(m => m.DurationInDays);
                    break;
                case "duration_desc":
                    membershipsQuery = membershipsQuery.OrderByDescending(m => m.DurationInDays);
                    break;
                case "name":
                    membershipsQuery = membershipsQuery.OrderBy(m => m.Title);
                    break;
                default:
                    membershipsQuery = membershipsQuery.OrderBy(m => m.Id);
                    break;
            }
            
            var memberships = await membershipsQuery.ToListAsync();
            var availableTypes = await _context.Memberships
                .Select(m => m.Type)
                .Distinct()
                .ToListAsync();
            
            var viewModel = new MembershipFilterViewModel
            {
                SearchTerm = searchTerm,
                TypeFilter = typeFilter,
                PriceFilter = priceFilter,
                SortBy = sortBy,
                Memberships = memberships,
                AvailableTypes = availableTypes,
                CurrentSubscription = subscription
            };
            
            ViewBag.Subscription = subscription;
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
                return NotFound();

            return View(membership); // Views/Membership/Details.cshtml
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
            {
                return NotFound();
            }
            
            // TypeItems'ı ViewBag'e ekleyin
            ViewBag.TypeItems = new[]
            {
                new SelectListItem("Havuz Üyeliği", MembershipType.Pool.ToString()),
                new SelectListItem("Fitness Üyeliği", MembershipType.Fitness.ToString()),
                new SelectListItem("Gold Üyelik", MembershipType.Gold.ToString())
            };
            
            return View(membership);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DurationInDays,Price,Type")] Membership membership, IFormFile? newImage)
        {
            if (id != membership.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMembership = await _context.Memberships.FindAsync(id);
                    if (existingMembership == null)
                    {
                        return NotFound();
                    }

                    // Varolan üyelik paketini güncelle
                    existingMembership.Title = membership.Title;
                    existingMembership.Description = membership.Description;
                    existingMembership.DurationInDays = membership.DurationInDays;
                    existingMembership.Price = membership.Price;
                    existingMembership.Type = membership.Type;

                    // Eğer fotoğraf yüklendiyse veritabanına yüklemek üzere dönüştür. jpg/png -> byte[]
                    if (newImage != null && newImage.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await newImage.CopyToAsync(ms);
                        existingMembership.Image = ms.ToArray();
                    }

                    _context.Update(existingMembership);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(ViewAllMemberships));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Memberships.Any(e => e.Id == membership.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            // Model geçerli değilse, ViewBag'i tekrar doldur
            ViewBag.TypeItems = new[]
            {
                new SelectListItem("Havuz Üyeliği", MembershipType.Pool.ToString()),
                new SelectListItem("Fitness Üyeliği", MembershipType.Fitness.ToString()),
                new SelectListItem("Gold Üyelik", MembershipType.Gold.ToString())
            };
            
            return View(membership);
        }

    }
}
