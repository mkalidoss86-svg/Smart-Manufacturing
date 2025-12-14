using InspectionWorker.Infrastructure.Messaging;

namespace InspectionWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqConsumer _consumer;
    private readonly RabbitMqPublisher _publisher;

    public Worker(
        ILogger<Worker> logger,
        RabbitMqConsumer consumer,
        RabbitMqPublisher publisher)
    {
        _logger = logger;
        _consumer = consumer;
        _publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inspection Worker starting at: {Time}", DateTimeOffset.Now);

        try
        {
            // Initialize publisher first
            await _publisher.InitializeAsync(stoppingToken);
            
            // Start consuming messages
            await _consumer.StartAsync(stoppingToken);

            _logger.LogInformation("Inspection Worker fully initialized and ready to process messages");

            // Keep the worker running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Inspection Worker is stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Inspection Worker");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inspection Worker is stopping gracefully");

        await _consumer.StopAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
