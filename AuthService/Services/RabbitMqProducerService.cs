using System.Text;
using System.Text.Json;
using AuthService.Configs;
using AuthService.Entities.DTO;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AuthService.Services
{
    public class RabbitMqProducerService(IOptions<RabbitMqSettings> rabbitMqConfig) : IRabbitMqProducerService
    {
        private const string EXCHANGE_FOR_USER_EVENTS = "user.events";
        private const string ROUTE_FOR_REGISTERED = "user.registered";

        private readonly RabbitMqSettings rabbitMqConfig = rabbitMqConfig.Value;

        public async Task PublishUserRegisteredAsync(UserRegisteredEvent evt)
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqConfig.HostName,
                UserName = rabbitMqConfig.UserName,
                Password = rabbitMqConfig.Password,
                Port = rabbitMqConfig.Port
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: EXCHANGE_FOR_USER_EVENTS,
                type: ExchangeType.Topic,
                durable: true
            );

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));

            await channel.BasicPublishAsync(
                exchange: EXCHANGE_FOR_USER_EVENTS,
                routingKey: ROUTE_FOR_REGISTERED,
                body: body
            );
        }

        public void Dispose()
        {

        }
    }
}