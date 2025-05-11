using System.ComponentModel.DataAnnotations;

namespace GymApp_v1.ViewModels
{
    public class UserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = "Member"; // Default role
        
        public IFormFile? ProfilePicture { get; set; }
        
        public int? MembershipId { get; set; }
    }
}