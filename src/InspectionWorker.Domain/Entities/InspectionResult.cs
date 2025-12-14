namespace InspectionWorker.Domain.Entities;

public class InspectionResult
{
    public string RequestId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public InspectionStatus Status { get; set; }
    public DateTime InspectedAt { get; set; }
    public string? DefectType { get; set; }
    public double ConfidenceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
