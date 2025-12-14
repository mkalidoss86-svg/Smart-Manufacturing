namespace ManufacturingDataSimulator.Application.DTOs;

public record ManufacturingEventDto(
    Guid Id,
    string ProductionLine,
    DateTime Timestamp,
    string BatchId,
    string ProductId,
    string DefectType,
    string Severity,
    string Status,
    double? ConfidenceScore,
    Dictionary<string, object>? Metadata
);

public record CreateManufacturingEventRequest(
    string ProductionLine,
    string BatchId,
    string ProductId,
    string DefectType,
    string Severity,
    string Status,
    double? ConfidenceScore,
    Dictionary<string, object>? Metadata
);

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record ProductionLineStats(
    string ProductionLine,
    int TotalEvents,
    int PassCount,
    int FailCount,
    int WarningCount,
    double DefectRate
);
