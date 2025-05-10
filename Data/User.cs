using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp_v1.Data
{
    public class User
    {
         // id => Birincil anahtar
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(6)]
        public required string Password { get; set; }
        
        // Profile fotoğrafını BLOB olarak saklar (MySQL LONGBLOB)
        // Entity Framework Core, byte[] tipindeki özellikleri varsayılan olarak BLOB/VARBINARY sütunlarına eşler. 
        [Column(TypeName = "longblob")]
        public byte[]? ProfilePicture { get; set; }
        // Kullanıcının rolü (Admin, Member vb.)
        [Required]
        public required string Role { get; set; } = string.Empty;

        // Sistemde görüntülenecek kullanıcı adı
        [Required]
        public required string Username { get; set; } = string.Empty;

        public int? MembershipId { get; set; }
        public Membership? Membership { get; set; }

        public List<Subscription> Subscriptions { get; set; } = new();


    }
}
