using InspectionWorker.Domain.Entities;

namespace InspectionWorker.Application.Interfaces;

public interface IInspectionService
{
    Task<InspectionResult> ProcessInspectionAsync(InspectionRequest request, CancellationToken cancellationToken = default);
}
