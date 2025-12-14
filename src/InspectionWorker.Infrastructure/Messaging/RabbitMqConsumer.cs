using System.Text;
using System.Text.Json;
using InspectionWorker.Application.Interfaces;
using InspectionWorker.Domain.Entities;
using InspectionWorker.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace InspectionWorker.Infrastructure.Messaging;

public class RabbitMqConsumer : IDisposable
{
    private readonly IInspectionService _inspectionService;
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    public RabbitMqConsumer(
        IInspectionService inspectionService,
        RabbitMqPublisher publisher,
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqConsumer> logger)
    {
        _inspectionService = inspectionService;
        _publisher = publisher;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RabbitMQ consumer");

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare dead letter exchange and queue
        _channel.ExchangeDeclare(_settings.DeadLetterExchange, ExchangeType.Direct, durable: true);
        _channel.QueueDeclare(_settings.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_settings.DeadLetterQueue, _settings.DeadLetterExchange, _settings.DeadLetterQueue);

        // Declare main queue with dead letter settings
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", _settings.DeadLetterExchange },
            { "x-dead-letter-routing-key", _settings.DeadLetterQueue }
        };
        _channel.QueueDeclare(_settings.InspectionRequestQueue, durable: true, exclusive: false, autoDelete: false, args);

        _channel.BasicQos(0, (ushort)_settings.PrefetchCount, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            await HandleMessageAsync(ea, cancellationToken);
        };

        _channel.BasicConsume(_settings.InspectionRequestQueue, autoAck: false, consumer);

        _logger.LogInformation("RabbitMQ consumer started and listening on queue: {Queue}", _settings.InspectionRequestQueue);
        
        await Task.CompletedTask;
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        var retryCount = GetRetryCount(ea.BasicProperties);
        
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            _logger.LogInformation("Received message with DeliveryTag: {DeliveryTag}, RetryCount: {RetryCount}", 
                ea.DeliveryTag, retryCount);

            var request = JsonSerializer.Deserialize<InspectionRequest>(message);
            if (request == null)
            {
                _logger.LogWarning("Failed to deserialize message. Moving to dead letter queue.");
                _channel!.BasicNack(ea.DeliveryTag, false, false);
                return;
            }

            // Process the inspection
            var result = await _inspectionService.ProcessInspectionAsync(request, cancellationToken);

            // Publish result
            await _publisher.PublishResultAsync(result, cancellationToken);

            // Acknowledge the message
            _channel!.BasicAck(ea.DeliveryTag, false);
            
            _logger.LogInformation("Successfully processed and acknowledged message {RequestId}", request.RequestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message. RetryCount: {RetryCount}", retryCount);

            if (retryCount >= _settings.MaxRetryAttempts)
            {
                _logger.LogWarning("Max retry attempts reached. Moving message to dead letter queue.");
                _channel!.BasicNack(ea.DeliveryTag, false, false);
            }
            else
            {
                _logger.LogInformation("Requeuing message for retry. RetryCount: {RetryCount}", retryCount);
                
                // Increment retry count and republish
                var newProperties = _channel!.CreateBasicProperties();
                newProperties.Headers = new Dictionary<string, object>
                {
                    { "x-retry-count", retryCount + 1 }
                };

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: _settings.InspectionRequestQueue,
                    basicProperties: newProperties,
                    body: ea.Body);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
        }
    }

    private int GetRetryCount(IBasicProperties properties)
    {
        if (properties.Headers != null && properties.Headers.TryGetValue("x-retry-count", out var value))
        {
            return value switch
            {
                int intValue => intValue,
                byte[] byteValue when byteValue.Length >= 4 => BitConverter.ToInt32(byteValue, 0),
                _ => 0
            };
        }
        return 0;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumer");
        
        _channel?.Close();
        _connection?.Close();
        
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
