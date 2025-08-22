using AuthService.Entities.DTO;

namespace AuthService.Services
{
    public interface IRabbitMqProducerService : IHostedService, IAsyncDisposable
    {
        Task PublishUserRegisteredAsync(UserRegisteredEvent evt);
    }
}