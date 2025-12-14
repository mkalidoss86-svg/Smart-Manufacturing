using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces;

public interface INotificationPublisher
{
    Task PublishInspectionUpdateAsync(InspectionResult inspectionResult, CancellationToken cancellationToken = default);
    Task PublishEventAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken = default);
}
