using NotificationService.Domain.Entities;

namespace NotificationService.Application.Observers;

public interface IInspectionResultObserver
{
    Task OnInspectionResultReceivedAsync(InspectionResult inspectionResult, CancellationToken cancellationToken = default);
}
