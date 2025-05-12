using System.ComponentModel.DataAnnotations;

namespace GymApp_v1.ViewModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "E-Posta alanı gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Şifre alanı gereklidir.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Kullanıcı adı alanı gereklidir.")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Rol seçmeniz zorunludur.")]
        public string Role { get; set; } = "Member"; // Default role ataması
        
        public IFormFile? ProfilePicture { get; set; }
        
        public int? MembershipId { get; set; }
    }
}