namespace VisionFlow.Application.DTOs;

public class ProductionQualityEventDto
{
    public string ProductId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public string? StationId { get; set; }
    public Dictionary<string, double> QualityMetrics { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object>? AdditionalData { get; set; }
}
