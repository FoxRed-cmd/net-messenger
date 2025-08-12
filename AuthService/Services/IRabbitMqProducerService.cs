using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Entities.DTO;

namespace AuthService.Services
{
    public interface IRabbitMqProducerService : IDisposable
    {
        Task PublishUserRegisteredAsync(UserRegisteredEvent evt);
    }
}