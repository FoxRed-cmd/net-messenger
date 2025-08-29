using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProfileService.Entities
{
    [Table("profiles")]
    public class Profile
    {
        [Key]
        public Guid Id { get; set; }

        [Column("display_name")]
        public string DisplayName { get; set; } = null!;

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("status")]
        public string? Status { get; set; }
    }
}