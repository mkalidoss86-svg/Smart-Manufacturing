using ManufacturingDataSimulator.Domain.Entities;
using ManufacturingDataSimulator.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManufacturingDataSimulator.Infrastructure.Persistence;

public class ManufacturingEventRepository : IManufacturingEventRepository
{
    private readonly ManufacturingDbContext _context;
    private readonly ILogger<ManufacturingEventRepository> _logger;

    public ManufacturingEventRepository(
        ManufacturingDbContext context,
        ILogger<ManufacturingEventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ManufacturingEvent> AddAsync(ManufacturingEvent evt)
    {
        try
        {
            _context.ManufacturingEvents.Add(evt);
            await _context.SaveChangesAsync();
            
            _logger.LogDebug("Added manufacturing event {Id}", evt.Id);
            
            return evt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding manufacturing event");
            throw;
        }
    }

    public async Task<ManufacturingEvent?> GetByIdAsync(Guid id)
    {
        return await _context.ManufacturingEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<(IEnumerable<ManufacturingEvent> Events, int TotalCount)> QueryAsync(
        string? productionLine = null,
        string? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.ManufacturingEvents.AsNoTracking();

        if (!string.IsNullOrEmpty(productionLine))
        {
            query = query.Where(e => e.ProductionLine == productionLine);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(e => e.Status.ToString() == status);
        }

        if (startTime.HasValue)
        {
            query = query.Where(e => e.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(e => e.Timestamp <= endTime.Value);
        }

        var totalCount = await query.CountAsync();

        var events = await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (events, totalCount);
    }

    public async Task<IEnumerable<ManufacturingEvent>> GetRecentEventsAsync(int count = 100)
    {
        return await _context.ManufacturingEvents
            .AsNoTracking()
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetProductionLineStatsAsync()
    {
        return await _context.ManufacturingEvents
            .GroupBy(e => e.ProductionLine)
            .Select(g => new { Line = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Line, x => x.Count);
    }
}
