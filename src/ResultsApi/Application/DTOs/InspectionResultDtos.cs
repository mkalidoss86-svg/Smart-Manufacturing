using System.ComponentModel.DataAnnotations;

namespace ResultsApi.Application.DTOs;

public record CreateInspectionResultRequest(
    [Required(ErrorMessage = "LineId is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "LineId must be between 1 and 100 characters")]
    string LineId,
    
    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Status must be between 1 and 50 characters")]
    string Status,
    
    [Required(ErrorMessage = "Timestamp is required")]
    DateTime Timestamp,
    
    [StringLength(100, ErrorMessage = "ProductId cannot exceed 100 characters")]
    string? ProductId = null,
    
    [StringLength(100, ErrorMessage = "DefectType cannot exceed 100 characters")]
    string? DefectType = null,
    
    [Range(0.0, 1.0, ErrorMessage = "ConfidenceScore must be between 0.0 and 1.0")]
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
