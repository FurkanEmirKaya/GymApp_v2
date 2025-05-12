using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GymApp_v1.Data;

namespace GymApp_v1.Models
{
    public class MembershipViewModel
    {
        [Required(ErrorMessage = "Başlık alanı gereklidir.")]
        public required string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama alanı gereklidir.")]
        public required string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat alanı gereklidir.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0.0m;

        [Required(ErrorMessage = "Süre alanı gereklidir.")]
        public int DurationInDays { get; set; } = 0;

        [Required(ErrorMessage = "Üyelik türü seçmeniz zorunludur.")]
        public MembershipType Type { get; set; } = MembershipType.None;

        [Required(ErrorMessage = "Bir görsel seçmelisiniz.")]
        public required IFormFile Image { get; set; } = null!;
    }
}
