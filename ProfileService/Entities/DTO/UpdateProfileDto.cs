namespace ProfileService.Entities.DTO
{
    public class UpdateProfileDto
    {
        public required string FirstName { get; set; }
        public string? LastName { get; set; }
        public required string UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
    }
}