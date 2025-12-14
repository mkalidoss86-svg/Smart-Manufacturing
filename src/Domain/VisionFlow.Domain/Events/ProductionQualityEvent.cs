namespace VisionFlow.Domain.Events;

public class ProductionQualityEvent
{
    public string EventId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string LineId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public string? StationId { get; set; }
    public Dictionary<string, double> QualityMetrics { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object>? AdditionalData { get; set; }
}
