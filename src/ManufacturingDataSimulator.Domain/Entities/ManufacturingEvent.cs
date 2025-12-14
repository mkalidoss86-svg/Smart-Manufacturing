using ManufacturingDataSimulator.Domain.Enums;

namespace ManufacturingDataSimulator.Domain.Entities;

public class ManufacturingEvent
{
    public Guid Id { get; set; }
    public string ProductionLine { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string BatchId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DefectType DefectType { get; set; }
    public DefectSeverity Severity { get; set; }
    public QualityStatus Status { get; set; }
    public double? ConfidenceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
