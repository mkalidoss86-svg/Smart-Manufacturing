using ManufacturingDataSimulator.Domain.Entities;

namespace ManufacturingDataSimulator.Domain.Interfaces;

public interface IManufacturingEventRepository
{
    Task<ManufacturingEvent> AddAsync(ManufacturingEvent evt);
    Task<ManufacturingEvent?> GetByIdAsync(Guid id);
    Task<(IEnumerable<ManufacturingEvent> Events, int TotalCount)> QueryAsync(
        string? productionLine = null,
        string? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int page = 1,
        int pageSize = 50
    );
    Task<IEnumerable<ManufacturingEvent>> GetRecentEventsAsync(int count = 100);
    Task<Dictionary<string, int>> GetProductionLineStatsAsync();
}
