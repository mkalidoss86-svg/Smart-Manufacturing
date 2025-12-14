namespace NotificationService.Application.DTOs;

public class MissedEventsRequestDto
{
    public long LastSequenceNumber { get; set; }
    public int MaxCount { get; set; } = 100;
}
