using InspectionWorker.Application.Services;
using InspectionWorker.Application.Strategies;
using InspectionWorker.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InspectionWorker.Tests;

public class InspectionServiceTests
{
    [Fact]
    public async Task ProcessInspectionAsync_ReturnsResult()
    {
        // Arrange
        var strategy = new SimulatedInspectionStrategy(NullLogger<SimulatedInspectionStrategy>.Instance);
        var service = new InspectionService(strategy, NullLogger<InspectionService>.Instance);
        
        var request = new InspectionRequest
        {
            RequestId = "test-service-001",
            ProductId = "PROD-123",
            BatchId = "BATCH-456",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await service.ProcessInspectionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.RequestId, result.RequestId);
    }

    [Fact]
    public async Task ProcessInspectionAsync_DuplicateRequest_ThrowsException()
    {
        // Arrange
        var strategy = new SimulatedInspectionStrategy(NullLogger<SimulatedInspectionStrategy>.Instance);
        var service = new InspectionService(strategy, NullLogger<InspectionService>.Instance);
        
        var request = new InspectionRequest
        {
            RequestId = "test-duplicate-001",
            ProductId = "PROD-123",
            BatchId = "BATCH-456",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await service.ProcessInspectionAsync(request);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.ProcessInspectionAsync(request));
    }

    [Fact]
    public async Task ProcessInspectionAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var strategy = new SimulatedInspectionStrategy(NullLogger<SimulatedInspectionStrategy>.Instance);
        var service = new InspectionService(strategy, NullLogger<InspectionService>.Instance);
        
        var request = new InspectionRequest
        {
            RequestId = "test-cancel-001",
            ProductId = "PROD-123",
            BatchId = "BATCH-456",
            Timestamp = DateTime.UtcNow
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await service.ProcessInspectionAsync(request, cts.Token));
    }
}
