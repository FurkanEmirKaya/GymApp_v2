using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp_v1.Data
{
    public class Membership
    {
        // Birincil anahtar
        [Key]
        public int Id { get; set; }

        // Üyelik başlığı
        [Required, StringLength(100)]
        public required string Title { get; set; }

        // Üyelik açıklaması
        [Required]
        public required string Description { get; set; }

        // Üyeliğin kaç gün süreceği
        [Required]
        public int DurationInDays { get; set; }

        // Üyelik resmi: veritabanında BLOB olarak saklanır
        // Entity Framework Core, byte[] tipindeki özellikleri varsayılan olarak BLOB/VARBINARY sütunlarına eşler.
        [Column(TypeName = "longblob")]
        public byte[]? Image { get; set; }

        // Fiyat: ondalıklı sayıyı DECIMAL(18,2) olarak tutar
        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Üyelik türü (ör. "Standard", "Premium" vb.)
        [Required, StringLength(50)]
        public required string Type { get; set; }

        public List<User> Users { get; set; }

            public Membership()
            {
                Title = string.Empty;
                Description = string.Empty;
                Type = string.Empty;
                Users = new List<User>();
            }
    }
}
