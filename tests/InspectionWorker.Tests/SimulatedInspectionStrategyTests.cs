using InspectionWorker.Application.Strategies;
using InspectionWorker.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InspectionWorker.Tests;

public class SimulatedInspectionStrategyTests
{
    [Fact]
    public void Inspect_ReturnsValidResult()
    {
        // Arrange
        var strategy = new SimulatedInspectionStrategy(NullLogger<SimulatedInspectionStrategy>.Instance);
        var request = new InspectionRequest
        {
            RequestId = "test-001",
            ProductId = "PROD-123",
            BatchId = "BATCH-456",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = strategy.Inspect(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.RequestId, result.RequestId);
        Assert.Equal(request.ProductId, result.ProductId);
        Assert.Equal(request.BatchId, result.BatchId);
        Assert.InRange(result.ConfidenceScore, 0.7, 1.0);
        Assert.Contains(result.Status, new[] { InspectionStatus.Pass, InspectionStatus.Defect, InspectionStatus.Anomaly });
    }

    [Fact]
    public void Inspect_DefectStatus_SetsDefectType()
    {
        // Arrange
        var strategy = new SimulatedInspectionStrategy(NullLogger<SimulatedInspectionStrategy>.Instance);
        var request = new InspectionRequest
        {
            RequestId = "test-002",
            ProductId = "PROD-124",
            BatchId = "BATCH-456",
            Timestamp = DateTime.UtcNow
        };

        // Act - Run multiple times to increase chance of getting a defect
        InspectionResult? defectResult = null;
        for (int i = 0; i < 50; i++)
        {
            var result = strategy.Inspect(new InspectionRequest
            {
                RequestId = $"test-{i}",
                ProductId = request.ProductId,
                BatchId = request.BatchId,
                Timestamp = DateTime.UtcNow
            });

            if (result.Status == InspectionStatus.Defect)
            {
                defectResult = result;
                break;
            }
        }

        // Assert
        if (defectResult != null)
        {
            Assert.NotNull(defectResult.DefectType);
            Assert.True(defectResult.Metadata.ContainsKey("DefectSeverity"));
        }
    }

    [Fact]
    public void Inspect_IncludesMetadata()
    {
        // Arrange
        var strategy = new SimulatedInspectionStrategy(NullLogger<SimulatedInspectionStrategy>.Instance);
        var request = new InspectionRequest
        {
            RequestId = "test-003",
            ProductId = "PROD-125",
            BatchId = "BATCH-456",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = strategy.Inspect(request);

        // Assert
        Assert.NotEmpty(result.Metadata);
        Assert.True(result.Metadata.ContainsKey("InspectionDurationMs"));
        Assert.True(result.Metadata.ContainsKey("InspectorVersion"));
    }
}
