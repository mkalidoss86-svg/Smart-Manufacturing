using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces;

public interface IEventStore
{
    Task StoreEventAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationEvent>> GetMissedEventsAsync(long lastSequenceNumber, int maxCount = 100, CancellationToken cancellationToken = default);
    Task<long> GetLatestSequenceNumberAsync(CancellationToken cancellationToken = default);
}
