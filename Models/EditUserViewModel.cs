using System.ComponentModel.DataAnnotations;

namespace GymApp_v1.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [MinLength(6)]
        public string? NewPassword { get; set; } // Nullable for optional password change
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        public IFormFile? NewProfilePicture { get; set; }
        
        public int? MembershipId { get; set; }
        
        // Display current profile picture
        public byte[]? CurrentProfilePicture { get; set; }
    }
}