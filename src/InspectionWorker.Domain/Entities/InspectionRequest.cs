namespace InspectionWorker.Domain.Entities;

public class InspectionRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Measurements { get; set; } = new();
}
