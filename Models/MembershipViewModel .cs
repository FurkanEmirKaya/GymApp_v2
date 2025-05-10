using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GymApp_v1.Data;

namespace GymApp_v1.Models
{
    public class MembershipViewModel
    {
        [Required]
        public required string Title { get; set; } = string.Empty;

        [Required]
        public required string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0.0m;

        [Required]
        public int DurationInDays { get; set; } = 0;

        [Required]
        public MembershipType Type { get; set; } = MembershipType.None;

        public required IFormFile Image { get; set; } = null!;
    }
}