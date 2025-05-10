using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp_v1.Data
{
    public class Subscription
    {
        // Birincil anahtar
        [Key]
        public int Id { get; set; }

        // Aboneliğin başlangıç tarihi
        [Required]
        public DateTime StartDate { get; set; }

        // Aboneliğin bitiş tarihi
        [Required]
        public DateTime EndDate { get; set; }

        // İlişkili Membership kaydının dış anahtarı
        [Required]
        public int MembershipId { get; set; }

        // Navigasyon özelliği: hangi Membership alındı
        [ForeignKey(nameof(MembershipId))]
        public Membership Membership { get; set; } = null!;

        // İlişkili User kaydının foreign anahtarı
        [Required]
        public int UserId { get; set; }

        // Navigasyon özelliği: aboneliği hangi kullanıcı aldı
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
