namespace ProfileService.Entities.DTO
{
    public class CreateProfileDto
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastVisit { get; set; }
        public bool StatusOnline { get; set; }
    }
}