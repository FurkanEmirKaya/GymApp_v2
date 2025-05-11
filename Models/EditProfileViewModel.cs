// ViewModels/EditProfileViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace GymApp_v1.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır")]
        public string? NewPassword { get; set; }
        
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string? ConfirmPassword { get; set; }
        
        public IFormFile? NewProfilePicture { get; set; }
        
        // Mevcut profil resmini görüntülemek için
        public byte[]? CurrentProfilePicture { get; set; }
    }
}