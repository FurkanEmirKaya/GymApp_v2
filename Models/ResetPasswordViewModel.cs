using System.ComponentModel.DataAnnotations;

namespace GymApp_v1.Models
{
    public class ResetPasswordViewModel
    {
        public required string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public required string NewPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public required string ConfirmPassword { get; set; }
        
        // Token property'sini ekleyin
        public string Token { get; set; } = string.Empty;
    }
}