using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Services;

public interface IInspectionNotificationService
{
    Task ProcessInspectionResultAsync(InspectionUpdateDto inspectionUpdateDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationEvent>> GetMissedEventsAsync(long lastSequenceNumber, int maxCount = 100, CancellationToken cancellationToken = default);
    Task<long> GetLatestSequenceNumberAsync(CancellationToken cancellationToken = default);
}
