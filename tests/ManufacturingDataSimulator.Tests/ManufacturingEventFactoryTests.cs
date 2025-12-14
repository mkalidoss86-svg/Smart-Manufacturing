using ManufacturingDataSimulator.Application.Configuration;
using ManufacturingDataSimulator.Domain.Interfaces;
using ManufacturingDataSimulator.Infrastructure.Factories;
using Microsoft.Extensions.Options;
using Xunit;

namespace ManufacturingDataSimulator.Tests;

public class ManufacturingEventFactoryTests
{
    [Fact]
    public void CreateEvent_ShouldGenerateValidEvent()
    {
        // Arrange
        var settings = Options.Create(new SimulatorSettings
        {
            ProductionLines = new[] { "Line-A", "Line-B", "Line-C" },
            DefectPercentage = 10.0
        });
        
        var defectStrategy = new MockDefectDistributionStrategy();
        var factory = new ManufacturingEventFactory(defectStrategy, settings);
        
        // Act
        var evt = factory.CreateEvent();
        
        // Assert
        Assert.NotEqual(Guid.Empty, evt.Id);
        Assert.NotNull(evt.ProductionLine);
        Assert.NotEmpty(evt.ProductionLine);
        Assert.NotNull(evt.BatchId);
        Assert.NotEmpty(evt.BatchId);
        Assert.NotNull(evt.ProductId);
        Assert.NotEmpty(evt.ProductId);
        Assert.NotNull(evt.Metadata);
        Assert.True(evt.Metadata.Count > 0);
    }

    [Fact]
    public void CreateEvent_WithSeed_ShouldBeDeterministic()
    {
        // Arrange
        var settings = Options.Create(new SimulatorSettings
        {
            ProductionLines = new[] { "Line-A", "Line-B", "Line-C" },
            DefectPercentage = 10.0,
            RandomSeed = 12345
        });
        
        var defectStrategy = new MockDefectDistributionStrategy();
        var factory = new ManufacturingEventFactory(defectStrategy, settings);
        
        // Act
        var evt1 = factory.CreateEvent(999);
        var evt2 = factory.CreateEvent(999);
        
        // Assert - Same seed should produce similar properties (not ID which is always unique)
        Assert.Equal(evt1.ProductionLine, evt2.ProductionLine);
    }

    [Fact]
    public void CreateEvent_ShouldUseProductionLinesFromSettings()
    {
        // Arrange
        var customLines = new[] { "Custom-Line-1", "Custom-Line-2" };
        var settings = Options.Create(new SimulatorSettings
        {
            ProductionLines = customLines,
            DefectPercentage = 10.0
        });
        
        var defectStrategy = new MockDefectDistributionStrategy();
        var factory = new ManufacturingEventFactory(defectStrategy, settings);
        
        // Act
        var events = Enumerable.Range(0, 100)
            .Select(_ => factory.CreateEvent())
            .ToList();
        
        // Assert
        Assert.All(events, evt => Assert.Contains(evt.ProductionLine, customLines));
    }
}

public class MockDefectDistributionStrategy : IDefectDistributionStrategy
{
    public double DefectProbability => 0.1;

    public (Domain.Enums.DefectType Type, Domain.Enums.DefectSeverity Severity, Domain.Enums.QualityStatus Status) GenerateDefect(double randomValue)
    {
        return (Domain.Enums.DefectType.None, Domain.Enums.DefectSeverity.None, Domain.Enums.QualityStatus.Pass);
    }
}
