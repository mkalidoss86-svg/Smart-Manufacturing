namespace ResultsApi.Domain;

public class InspectionResult
{
    public Guid Id { get; set; }
    public string LineId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ProductId { get; set; }
    public string? DefectType { get; set; }
    public double? ConfidenceScore { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
