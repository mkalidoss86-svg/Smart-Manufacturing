using ManufacturingDataSimulator.Application.Configuration;
using ManufacturingDataSimulator.Domain.Entities;
using ManufacturingDataSimulator.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace ManufacturingDataSimulator.Infrastructure.Factories;

public class ManufacturingEventFactory : IEventFactory
{
    private readonly IDefectDistributionStrategy _defectStrategy;
    private readonly SimulatorSettings _settings;
    private readonly Random _random;
    private static int _batchCounter = 0;

    public ManufacturingEventFactory(
        IDefectDistributionStrategy defectStrategy,
        IOptions<SimulatorSettings> settings)
    {
        _defectStrategy = defectStrategy;
        _settings = settings.Value;
        _random = _settings.RandomSeed.HasValue 
            ? new Random(_settings.RandomSeed.Value) 
            : new Random();
    }

    public ManufacturingEvent CreateEvent(int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : _random;
        
        var productionLine = _settings.ProductionLines[random.Next(_settings.ProductionLines.Length)];
        var batchNumber = Interlocked.Increment(ref _batchCounter);
        var batchId = $"BATCH-{DateTime.UtcNow:yyyyMMdd}-{batchNumber:D6}";
        var productId = $"PROD-{random.Next(10000, 99999)}";
        
        var randomValue = random.NextDouble();
        var (defectType, severity, status) = _defectStrategy.GenerateDefect(randomValue);
        
        var confidenceScore = status == Domain.Enums.QualityStatus.Pass 
            ? 0.95 + (random.NextDouble() * 0.05)
            : 0.60 + (random.NextDouble() * 0.35);

        var metadata = new Dictionary<string, object>
        {
            { "Temperature", Math.Round(20 + (random.NextDouble() * 10), 2) },
            { "Humidity", Math.Round(40 + (random.NextDouble() * 20), 2) },
            { "Pressure", Math.Round(100 + (random.NextDouble() * 10), 2) },
            { "InspectionDuration", random.Next(50, 500) }
        };

        return new ManufacturingEvent
        {
            Id = Guid.NewGuid(),
            ProductionLine = productionLine,
            Timestamp = DateTime.UtcNow,
            BatchId = batchId,
            ProductId = productId,
            DefectType = defectType,
            Severity = severity,
            Status = status,
            ConfidenceScore = Math.Round(confidenceScore, 4),
            Metadata = metadata
        };
    }
}
