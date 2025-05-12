using GymApp_v1.Data;
using GymApp_v1.Models;
using GymApp_v1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace efCoreApp.AddControllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DataContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        
        public AdminController(DataContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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
            {
                ViewBag.TypeItems = new[]
                {
                    new SelectListItem("Havuz Üyeliği", MembershipType.Pool.ToString()),
                    new SelectListItem("Fitness Üyeliği", MembershipType.Fitness.ToString()),
                    new SelectListItem("Gold Üyelik", MembershipType.Gold.ToString())
                };
                return View(model);
            }

            var entity = new Membership
            {
                Title = model.Title,
                Description = model.Description,
                DurationInDays = model.DurationInDays,
                Type = model.Type.ToString(),
                Price = model.Price,
                Image = model.Image != null ? FileToBytes(model.Image) : null
            };

            _context.Memberships.Add(entity);
            await _context.SaveChangesAsync();
            
            // ViewAllMemberships'e yönlendirme
            return RedirectToAction("ViewAllMemberships", "Membership");
        }
        
        #region User Operations
        
        [HttpGet]
        public async Task<IActionResult> CreateUsers()
        {
            await LoadUserFormData();
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateUsers(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadUserFormData();
                return View(model);
            }
            
            // Email halihazırda var mı kontrol et
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                await LoadUserFormData();
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                Username = model.Username,
                Role = model.Role,
                // MembershipId'yi kontrol et - null veya 0 ise hiç atama yapma veya null ata
                MembershipId = model.MembershipId.HasValue && model.MembershipId.Value > 0 
                    ? model.MembershipId.Value 
                    : (int?)null, // Nullable int olarak null ata
                Password = string.Empty
            };
            
            // Hash password
            user.Password = _passwordHasher.HashPassword(user, model.Password);
            
            // Handle profile picture
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                user.ProfilePicture = FileToBytes(model.ProfilePicture);
            }
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("ListAllUsers");
        }

        [HttpGet]
        public async Task<IActionResult> ListAllUsers(string? searchTerm, string? roleFilter, string? membershipFilter, string? sortBy)
        {
            // Tüm kullanıcıları include ile al
            var usersQuery = _context.Users
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.Membership)
                .AsQueryable();
            
            // Arama
            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u => 
                    u.Username.Contains(searchTerm) || 
                    u.Email.Contains(searchTerm));
            }
            
            // Rol filtreleme
            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "all")
            {
                usersQuery = usersQuery.Where(u => u.Role == roleFilter);
            }
            
            // Üyelik filtreleme
            if (!string.IsNullOrEmpty(membershipFilter) && membershipFilter != "all")
            {
                if (membershipFilter == "none")
                {
                    // Üyeliği olmayanları göster
                    usersQuery = usersQuery.Where(u => u.MembershipId == null || u.MembershipId == 0 || !u.Subscriptions.Any() || !u.Subscriptions.Any(s => s.EndDate > DateTime.Now));
                }
                else if (membershipFilter == "active")
                {
                    // Aktif üyeliği olanları göster
                    usersQuery = usersQuery.Where(u => u.Subscriptions.Any(s => s.EndDate > DateTime.Now));
                }
                else if (int.TryParse(membershipFilter, out int membershipId))
                {
                    // Belirli bir üyelik tipine sahip olanları göster
                    usersQuery = usersQuery.Where(u => u.Subscriptions.Any(s => 
                        s.MembershipId == membershipId && s.EndDate > DateTime.Now));
                }
            }
            
            // Sıralama
            switch (sortBy)
            {
                case "username":
                    usersQuery = usersQuery.OrderBy(u => u.Username);
                    break;
                case "email":
                    usersQuery = usersQuery.OrderBy(u => u.Email);
                    break;
                case "role":
                    usersQuery = usersQuery.OrderBy(u => u.Role);
                    break;
                case "newest":
                    usersQuery = usersQuery.OrderByDescending(u => u.Id);
                    break;
                case "oldest":
                    usersQuery = usersQuery.OrderBy(u => u.Id);
                    break;
                default:
                    usersQuery = usersQuery.OrderBy(u => u.Id);
                    break;
            }
            
            var users = await usersQuery.ToListAsync();
            
            // Filtreleme seçenekleri için veri al
            var availableRoles = await _context.Users
                .Select(u => u.Role)
                .Distinct()
                .ToListAsync();
            
            var availableMemberships = await _context.Memberships
                .OrderBy(m => m.Title)
                .ToListAsync();
            
            var viewModel = new UserFilterViewModel
            {
                SearchTerm = searchTerm,
                RoleFilter = roleFilter,
                MembershipFilter = membershipFilter,
                SortBy = sortBy,
                Users = users,
                AvailableRoles = availableRoles,
                AvailableMemberships = availableMemberships
            };
            
            return View(viewModel);
        }    
        
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            
            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                MembershipId = user.MembershipId,
                CurrentProfilePicture = user.ProfilePicture
            };
            
            await LoadUserFormData();
            return View(viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadUserFormData();
                return View(model);
            }
            
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
                return NotFound();
            
            // Email halihazırda başka bir kullanıcı tarafından kullanılıyor mu 
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                await LoadUserFormData();
                return View(model);
            }
            
            user.Email = model.Email;
            user.Username = model.Username;
            user.Role = model.Role;
            // EditUser'da da aynı kontrolü uygula
            user.MembershipId = model.MembershipId.HasValue && model.MembershipId.Value > 0 
                ? model.MembershipId.Value 
                : (int?)null;
            
            // Eğer kullanıcı yeni şifre girdiyse veritabanına yollamak için şifrele
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            }
            
            // Eğer kullanıcı yeni profil fotoğrafı eklediyse veritabanına yollamak için dönüştür. jpg/png -> byte[]
            if (model.NewProfilePicture != null && model.NewProfilePicture.Length > 0)
            {
                user.ProfilePicture = FileToBytes(model.NewProfilePicture);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("ListAllUsers");
        }
        
        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
                
            return View(user);
        }
        
        [HttpPost, ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
                
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("ListAllUsers");
        }
        
        private async Task LoadUserFormData()
        {
            ViewBag.Roles = new SelectList(new List<string> { "Admin", "Member" });
            
            var memberships = await _context.Memberships.ToListAsync();
            ViewBag.Memberships = new SelectList(memberships, "Id", "Title");
        }
        
        #endregion

        private byte[] FileToBytes(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
}