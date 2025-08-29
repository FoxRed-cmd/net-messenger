
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ProfileService.Configs;
using ProfileService.Entities;
using ProfileService.Entities.DTO;
using ProfileService.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProfileService.Services
{
    public class RabbitMqConsumerService(
        IOptions<RabbitMqSettings> rabbitMqConfig,
        IProfileService profileService) : BackgroundService
    {
        private const string EXCHANGE_FOR_USER_EVENTS = "user.events";
        private const string ROUTE_FOR_REGISTERED = "user.registered";
        private const string QUEUE_FOR_REGISTERED = "user.registered.queue";

        private readonly RabbitMqSettings rabbitMqConfig = rabbitMqConfig.Value;
        private readonly IProfileService profileService = profileService;
        private IConnection? connection;
        private IChannel? channel;

        public override async Task StartAsync(CancellationToken cancellationToken)
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

            // Гарантируем что exchange и очередь существуют
            await channel.ExchangeDeclareAsync(EXCHANGE_FOR_USER_EVENTS, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(QUEUE_FOR_REGISTERED, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(QUEUE_FOR_REGISTERED, EXCHANGE_FOR_USER_EVENTS, ROUTE_FOR_REGISTERED, cancellationToken: cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (channel is null) throw new InvalidOperationException("RabbitMQ channel is not initialized.");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var evt = JsonSerializer.Deserialize<UserRegisteredEvent>(message);

                    await profileService.CreateProfileAsync(evt ?? throw new(nameof(evt)));

                    // подтверждаем что сообщение обработано
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Consumer] Error: {ex}");
                    // можно сделать BasicNack для повторной обработки
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync
            (
                queue: QUEUE_FOR_REGISTERED,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken

            );

            // держим consumer живым пока сервис работает
            await Task.Delay(-1, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (channel != null)
                await channel.CloseAsync(cancellationToken: cancellationToken);
            if (connection != null)
                await connection.CloseAsync(cancellationToken: cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}