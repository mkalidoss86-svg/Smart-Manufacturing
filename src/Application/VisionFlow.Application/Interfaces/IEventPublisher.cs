using VisionFlow.Domain.Events;

namespace VisionFlow.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(ProductionQualityEvent qualityEvent, CancellationToken cancellationToken = default);
}
