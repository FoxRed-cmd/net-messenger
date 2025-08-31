using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProfileService.Entities
{
    [Table("profiles")]
    public class Profile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column("first_name")]
        public required string FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [NotMapped]
        public string FullName
        {
            get => $"{FirstName} {LastName}".Trim();
            set
            {
                var names = value.Split(' ');
                FirstName = names[0];
                LastName = names.Length > 1 ? names[1] : null;
            }
        }

        [Required]
        [Column("email")]
        public required string Email { get; set; }

        [Required]
        [Column("display_name")]
        public required string UserName { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_visit")]
        public DateTime LastVisit { get; set; }

        [Column("status_online")]
        public bool StatusOnline { get; set; }

    }
}