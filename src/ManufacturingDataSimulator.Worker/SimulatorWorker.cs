using ManufacturingDataSimulator.Application.Configuration;
using ManufacturingDataSimulator.Application.Services;
using Microsoft.Extensions.Options;

namespace ManufacturingDataSimulator.Worker;

public class SimulatorWorker : BackgroundService
{
    private readonly ILogger<SimulatorWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly SimulatorSettings _settings;
    private int _burstCounter = 0;

    public SimulatorWorker(
        ILogger<SimulatorWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<SimulatorSettings> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Manufacturing Data Simulator Worker starting...");
        _logger.LogInformation("Settings - Event Rate: {Rate}/sec, Defect %: {Defect}%, Continuous: {Continuous}, Burst: {Burst}",
            _settings.EventRatePerSecond,
            _settings.DefectPercentage,
            _settings.ContinuousMode,
            _settings.EnableBurstMode);

        if (!_settings.ContinuousMode)
        {
            _logger.LogInformation("Continuous mode disabled. Worker will not generate events.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var eventRate = _settings.EventRatePerSecond;

                if (_settings.EnableBurstMode)
                {
                    _burstCounter++;
                    if (_burstCounter >= _settings.BurstIntervalSeconds)
                    {
                        _logger.LogInformation("Burst mode activated - generating {Multiplier}x events",
                            _settings.BurstMultiplier);
                        eventRate *= _settings.BurstMultiplier;
                        _burstCounter = 0;
                    }
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<SimulatorService>();
                    
                    for (int i = 0; i < eventRate; i++)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        await service.GenerateAndPersistEventAsync();
                    }
                }

                _logger.LogDebug("Generated {Count} events", eventRate);

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Simulator worker stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in simulator worker");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Manufacturing Data Simulator Worker stopped");
    }
}
