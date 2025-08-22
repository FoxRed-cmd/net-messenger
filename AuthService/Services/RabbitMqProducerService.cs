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
        private IConnection connection = null!;
        private IChannel channel = null!;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqConfig.HostName,
                UserName = rabbitMqConfig.UserName,
                Password = rabbitMqConfig.Password,
                Port = rabbitMqConfig.Port
            };

            connection = await factory.CreateConnectionAsync(cancellationToken);
            channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: EXCHANGE_FOR_USER_EVENTS,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken
            );
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (channel != null) await channel.CloseAsync(cancellationToken: cancellationToken);
            if (connection != null) await connection.CloseAsync(cancellationToken: cancellationToken);
        }

        public async Task PublishUserRegisteredAsync(UserRegisteredEvent evt)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));

            await channel.BasicPublishAsync(
                exchange: EXCHANGE_FOR_USER_EVENTS,
                routingKey: ROUTE_FOR_REGISTERED,
                body: body
            );
        }

        public async ValueTask DisposeAsync()
        {
            if (channel != null) await channel.DisposeAsync();
            if (connection != null) await connection.DisposeAsync();
        }
    }
}