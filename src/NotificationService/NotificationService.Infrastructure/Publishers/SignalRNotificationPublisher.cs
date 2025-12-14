using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Hubs;

namespace NotificationService.Infrastructure.Publishers;

public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<InspectionHub> _hubContext;

    public SignalRNotificationPublisher(IHubContext<InspectionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishInspectionUpdateAsync(InspectionResult inspectionResult, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group("InspectionUpdates")
            .SendAsync("InspectionUpdate", inspectionResult, cancellationToken);
    }

    public async Task PublishEventAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group("InspectionUpdates")
            .SendAsync("NotificationEvent", notificationEvent, cancellationToken);
    }
}
