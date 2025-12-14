using ResultsApi.Application.Interfaces;
using ResultsApi.Domain;
using System.Collections.Concurrent;

namespace ResultsApi.Infrastructure.Repositories;

public class InMemoryInspectionResultRepository : IInspectionResultRepository
{
    private readonly ConcurrentDictionary<Guid, InspectionResult> _results = new();
    private readonly ILogger<InMemoryInspectionResultRepository> _logger;

    public InMemoryInspectionResultRepository(ILogger<InMemoryInspectionResultRepository> logger)
    {
        _logger = logger;
    }

    public Task<InspectionResult> AddAsync(InspectionResult result)
    {
        if (result.Id == Guid.Empty)
        {
            result.Id = Guid.NewGuid();
        }

        if (_results.TryAdd(result.Id, result))
        {
            _logger.LogInformation("Added inspection result {Id} for line {LineId} with status {Status}", 
                result.Id, result.LineId, result.Status);
            return Task.FromResult(result);
        }

        throw new InvalidOperationException($"Failed to add inspection result with ID {result.Id}");
    }

    public Task<InspectionResult?> GetByIdAsync(Guid id)
    {
        _results.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<(IEnumerable<InspectionResult> Results, int TotalCount)> QueryAsync(
        string? lineId = null,
        string? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _results.Values.AsEnumerable();

        // Apply filters
        if (!string.IsNullOrEmpty(lineId))
        {
            query = query.Where(r => r.LineId.Equals(lineId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (startTime.HasValue)
        {
            query = query.Where(r => r.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(r => r.Timestamp <= endTime.Value);
        }

        // Get total count before pagination
        var totalCount = query.Count();

        // Apply pagination
        var results = query
            .OrderByDescending(r => r.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation("Query returned {Count} results out of {Total} (page {Page}, pageSize {PageSize})",
            results.Count, totalCount, page, pageSize);

        return Task.FromResult<(IEnumerable<InspectionResult>, int)>((results, totalCount));
    }
}
