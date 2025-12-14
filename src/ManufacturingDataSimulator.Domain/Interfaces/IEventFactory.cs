using ManufacturingDataSimulator.Domain.Entities;

namespace ManufacturingDataSimulator.Domain.Interfaces;

public interface IEventFactory
{
    ManufacturingEvent CreateEvent(int? seed = null);
}
