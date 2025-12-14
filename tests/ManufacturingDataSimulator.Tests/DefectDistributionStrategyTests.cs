using ManufacturingDataSimulator.Domain.Enums;
using ManufacturingDataSimulator.Infrastructure.Strategies;
using Xunit;

namespace ManufacturingDataSimulator.Tests;

public class DefectDistributionStrategyTests
{
    [Fact]
    public void ConfigurableDefectDistributionStrategy_ShouldGenerateNoDefect_WhenRandomValueAboveThreshold()
    {
        // Arrange
        var strategy = new ConfigurableDefectDistributionStrategy(10.0); // 10% defect rate
        
        // Act
        var (type, severity, status) = strategy.GenerateDefect(0.15); // 15% > 10%
        
        // Assert
        Assert.Equal(DefectType.None, type);
        Assert.Equal(DefectSeverity.None, severity);
        Assert.Equal(QualityStatus.Pass, status);
    }

    [Fact]
    public void ConfigurableDefectDistributionStrategy_ShouldGenerateDefect_WhenRandomValueBelowThreshold()
    {
        // Arrange
        var strategy = new ConfigurableDefectDistributionStrategy(10.0); // 10% defect rate
        
        // Act
        var (type, severity, status) = strategy.GenerateDefect(0.05); // 5% < 10%
        
        // Assert
        Assert.NotEqual(DefectType.None, type);
        Assert.NotEqual(DefectSeverity.None, severity);
        Assert.True(status == QualityStatus.Warning || status == QualityStatus.Fail);
    }

    [Fact]
    public void ConfigurableDefectDistributionStrategy_ShouldClampDefectPercentage()
    {
        // Arrange & Act
        var strategy1 = new ConfigurableDefectDistributionStrategy(-10.0);
        var strategy2 = new ConfigurableDefectDistributionStrategy(150.0);
        
        // Assert
        Assert.Equal(0.0, strategy1.DefectProbability);
        Assert.Equal(1.0, strategy2.DefectProbability);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    [InlineData(50.0)]
    [InlineData(100.0)]
    public void ConfigurableDefectDistributionStrategy_ShouldSetCorrectProbability(double percentage)
    {
        // Arrange & Act
        var strategy = new ConfigurableDefectDistributionStrategy(percentage);
        
        // Assert
        Assert.Equal(percentage / 100.0, strategy.DefectProbability);
    }
}
