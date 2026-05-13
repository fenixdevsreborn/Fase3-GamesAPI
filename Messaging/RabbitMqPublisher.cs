using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ms_games.Messaging;

public sealed class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable, IDisposable
{
    private readonly IConnection _connection;
    private readonly IConfiguration _configuration;
    private IChannel? _channel;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMq:Port"] ?? "5672"),
            UserName = _configuration["RabbitMq:Username"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest"
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    public async Task PublishAsync<T>(string queueName, T message) where T : class
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException("RabbitMQ queue name is not configured.");
        }

        await EnsureChannelAsync();

        await _channel!.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }

    private async Task EnsureChannelAsync()
    {
        if (_channel is null || _channel.IsClosed)
        {
            _channel = await _connection.CreateChannelAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            if (_channel.IsOpen)
            {
                await _channel.CloseAsync();
            }

            await _channel.DisposeAsync();
        }

        if (_connection.IsOpen)
        {
            await _connection.CloseAsync();
        }

        await _connection.DisposeAsync();
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}
