using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AuthService.Entities
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("token")]
        public required string Token { get; set; }

        [Column("user_id")]
        public Guid? UserId { get; set; }

        [Column("expires")]
        public DateTime Expires { get; set; }

        [Column("is_revoked")]
        public bool IsRevoked { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; }

        public virtual User? User { get; set; }
    }
}