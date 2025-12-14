using System.Text;
using System.Text.Json;
using InspectionWorker.Domain.Entities;
using InspectionWorker.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace InspectionWorker.Infrastructure.Messaging;

public class RabbitMqPublisher : IDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    public RabbitMqPublisher(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing RabbitMQ publisher");

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare result queue
        _channel.QueueDeclare(_settings.InspectionResultQueue, durable: true, exclusive: false, autoDelete: false);

        _logger.LogInformation("RabbitMQ publisher initialized");
        
        await Task.CompletedTask;
    }

    public async Task PublishResultAsync(InspectionResult result, CancellationToken cancellationToken)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Publisher not initialized. Call InitializeAsync first.");
        }

        var message = JsonSerializer.Serialize(result);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = result.RequestId;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: "",
            routingKey: _settings.InspectionResultQueue,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Published inspection result for RequestId: {RequestId}, Status: {Status}", 
            result.RequestId, result.Status);
        
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _channel?.Dispose();
        _connection?.Dispose();
        _disposed = true;
        
        GC.SuppressFinalize(this);
    }
}
