namespace ResultsApi.Application.DTOs;

public record CreateInspectionResultRequest(
    string LineId,
    string Status,
    DateTime Timestamp,
    string? ProductId = null,
    string? DefectType = null,
    double? ConfidenceScore = null,
    Dictionary<string, object>? Metadata = null
);

public record InspectionResultResponse(
    Guid Id,
    string LineId,
    string Status,
    DateTime Timestamp,
    string? ProductId,
    string? DefectType,
    double? ConfidenceScore,
    Dictionary<string, object>? Metadata
);

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record InspectionResultQuery(
    string? LineId = null,
    string? Status = null,
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    int Page = 1,
    int PageSize = 50
);
