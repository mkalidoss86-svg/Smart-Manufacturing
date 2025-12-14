using ManufacturingDataSimulator.Domain.Entities;
using ManufacturingDataSimulator.Domain.Enums;
using ManufacturingDataSimulator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ManufacturingDataSimulator.Tests;

public class ManufacturingEventRepositoryTests
{
    private ManufacturingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ManufacturingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ManufacturingDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEvent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ManufacturingEventRepository(context, 
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ManufacturingEventRepository>.Instance);
        
        var evt = new ManufacturingEvent
        {
            Id = Guid.NewGuid(),
            ProductionLine = "Line-A",
            Timestamp = DateTime.UtcNow,
            BatchId = "BATCH-001",
            ProductId = "PROD-001",
            DefectType = DefectType.None,
            Severity = DefectSeverity.None,
            Status = QualityStatus.Pass
        };
        
        // Act
        var result = await repository.AddAsync(evt);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(evt.Id, result.Id);
        
        var retrieved = await repository.GetByIdAsync(evt.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(evt.ProductionLine, retrieved.ProductionLine);
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByProductionLine()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ManufacturingEventRepository(context,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ManufacturingEventRepository>.Instance);
        
        // Add events for different lines
        await repository.AddAsync(CreateTestEvent("Line-A"));
        await repository.AddAsync(CreateTestEvent("Line-A"));
        await repository.AddAsync(CreateTestEvent("Line-B"));
        
        // Act
        var (events, totalCount) = await repository.QueryAsync(productionLine: "Line-A");
        
        // Assert
        Assert.Equal(2, totalCount);
        Assert.All(events, e => Assert.Equal("Line-A", e.ProductionLine));
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByStatus()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ManufacturingEventRepository(context,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ManufacturingEventRepository>.Instance);
        
        await repository.AddAsync(CreateTestEvent("Line-A", QualityStatus.Pass));
        await repository.AddAsync(CreateTestEvent("Line-A", QualityStatus.Fail));
        await repository.AddAsync(CreateTestEvent("Line-A", QualityStatus.Pass));
        
        // Act
        var (events, totalCount) = await repository.QueryAsync(status: "Pass");
        
        // Assert
        Assert.Equal(2, totalCount);
        Assert.All(events, e => Assert.Equal(QualityStatus.Pass, e.Status));
    }

    [Fact]
    public async Task QueryAsync_ShouldSupportPagination()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ManufacturingEventRepository(context,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ManufacturingEventRepository>.Instance);
        
        for (int i = 0; i < 25; i++)
        {
            await repository.AddAsync(CreateTestEvent("Line-A"));
        }
        
        // Act
        var (page1, totalCount1) = await repository.QueryAsync(page: 1, pageSize: 10);
        var (page2, totalCount2) = await repository.QueryAsync(page: 2, pageSize: 10);
        
        // Assert
        Assert.Equal(25, totalCount1);
        Assert.Equal(25, totalCount2);
        Assert.Equal(10, page1.Count());
        Assert.Equal(10, page2.Count());
    }

    [Fact]
    public async Task GetRecentEventsAsync_ShouldReturnMostRecentEvents()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ManufacturingEventRepository(context,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ManufacturingEventRepository>.Instance);
        
        var oldEvent = CreateTestEvent("Line-A");
        oldEvent.Timestamp = DateTime.UtcNow.AddHours(-2);
        await repository.AddAsync(oldEvent);
        
        var newEvent = CreateTestEvent("Line-A");
        newEvent.Timestamp = DateTime.UtcNow;
        await repository.AddAsync(newEvent);
        
        // Act
        var events = (await repository.GetRecentEventsAsync(10)).ToList();
        
        // Assert
        Assert.Equal(2, events.Count);
        Assert.Equal(newEvent.Id, events[0].Id); // Most recent first
        Assert.Equal(oldEvent.Id, events[1].Id);
    }

    private ManufacturingEvent CreateTestEvent(string productionLine, QualityStatus status = QualityStatus.Pass)
    {
        return new ManufacturingEvent
        {
            Id = Guid.NewGuid(),
            ProductionLine = productionLine,
            Timestamp = DateTime.UtcNow,
            BatchId = $"BATCH-{Guid.NewGuid():N}",
            ProductId = $"PROD-{Guid.NewGuid():N}",
            DefectType = status == QualityStatus.Pass ? DefectType.None : DefectType.Scratch,
            Severity = status == QualityStatus.Pass ? DefectSeverity.None : DefectSeverity.Low,
            Status = status
        };
    }
}
