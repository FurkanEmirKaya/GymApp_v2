// ViewModels/ForgotPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace GymApp_v1.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = string.Empty;
    }
}