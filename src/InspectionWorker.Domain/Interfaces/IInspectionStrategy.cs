using InspectionWorker.Domain.Entities;

namespace InspectionWorker.Domain.Interfaces;

public interface IInspectionStrategy
{
    InspectionResult Inspect(InspectionRequest request);
}
