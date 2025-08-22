namespace ProfileService.Entities.DTO
{
    public class UserRegisteredEvent
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
    }
}