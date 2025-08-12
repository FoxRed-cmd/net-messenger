using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthService.Entities.Enums;

namespace AuthService.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("salt")]
        public string Salt { get; set; } = null!;

        [Column("role")]
        public Role Role { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}