using ManufacturingDataSimulator.Domain.Entities;

namespace ManufacturingDataSimulator.Application.Services;

public interface IManufacturingEventPublisher
{
    Task PublishEventAsync(ManufacturingEvent evt);
}
