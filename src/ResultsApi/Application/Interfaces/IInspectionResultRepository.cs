using ResultsApi.Domain;

namespace ResultsApi.Application.Interfaces;

public interface IInspectionResultRepository
{
    Task<InspectionResult> AddAsync(InspectionResult result);
    Task<InspectionResult?> GetByIdAsync(Guid id);
    Task<(IEnumerable<InspectionResult> Results, int TotalCount)> QueryAsync(
        string? lineId = null,
        string? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int page = 1,
        int pageSize = 50
    );
}
