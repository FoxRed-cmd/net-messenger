namespace ProfileService.Entities.DTO
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
    }
}