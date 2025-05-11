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
            
            // Check if email already exists
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
                MembershipId = model.MembershipId,
                Password = String.Empty
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
            
            // Check if email is already used by another user
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                await LoadUserFormData();
                return View(model);
            }
            
            user.Email = model.Email;
            user.Username = model.Username;
            user.Role = model.Role;
            user.MembershipId = model.MembershipId;
            
            // Update password if new password is provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            }
            
            // Update profile picture if new one is uploaded
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
        
        // Mevcut diğer metodlarınız...
        
        private byte[] FileToBytes(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
}