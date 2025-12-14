namespace NotificationService.Domain.Entities;

public class NotificationEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public InspectionResult? Payload { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long SequenceNumber { get; set; }
}
