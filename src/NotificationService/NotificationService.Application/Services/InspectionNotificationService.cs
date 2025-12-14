using NotificationService.Application.DTOs;
using NotificationService.Application.Observers;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Services;

public class InspectionNotificationService : IInspectionNotificationService
{
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IEventStore _eventStore;
    private readonly List<IInspectionResultObserver> _observers = new();
    private long _sequenceNumber = 0;

    public InspectionNotificationService(
        INotificationPublisher notificationPublisher,
        IEventStore eventStore)
    {
        _notificationPublisher = notificationPublisher;
        _eventStore = eventStore;
    }

    public void Subscribe(IInspectionResultObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Unsubscribe(IInspectionResultObserver observer)
    {
        _observers.Remove(observer);
    }

    public async Task ProcessInspectionResultAsync(InspectionUpdateDto inspectionUpdateDto, CancellationToken cancellationToken = default)
    {
        var inspectionResult = MapToInspectionResult(inspectionUpdateDto);
        
        // Create and store event with sequence number
        var notificationEvent = new NotificationEvent
        {
            EventType = "InspectionUpdate",
            Payload = inspectionResult,
            SequenceNumber = Interlocked.Increment(ref _sequenceNumber)
        };

        await _eventStore.StoreEventAsync(notificationEvent, cancellationToken);

        // Notify all observers
        foreach (var observer in _observers)
        {
            await observer.OnInspectionResultReceivedAsync(inspectionResult, cancellationToken);
        }

        // Publish to SignalR clients
        await _notificationPublisher.PublishInspectionUpdateAsync(inspectionResult, cancellationToken);
    }

    public async Task<IEnumerable<NotificationEvent>> GetMissedEventsAsync(long lastSequenceNumber, int maxCount = 100, CancellationToken cancellationToken = default)
    {
        return await _eventStore.GetMissedEventsAsync(lastSequenceNumber, maxCount, cancellationToken);
    }

    public async Task<long> GetLatestSequenceNumberAsync(CancellationToken cancellationToken = default)
    {
        return await _eventStore.GetLatestSequenceNumberAsync(cancellationToken);
    }

    private static InspectionResult MapToInspectionResult(InspectionUpdateDto dto)
    {
        return new InspectionResult
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            Status = dto.Status,
            Severity = dto.Severity,
            Message = dto.Message,
            Timestamp = dto.Timestamp,
            Metadata = dto.Metadata ?? new Dictionary<string, object>()
        };
    }
}
