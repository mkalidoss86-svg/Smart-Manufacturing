using InspectionWorker.Domain.Entities;
using InspectionWorker.Domain.Enums;
using InspectionWorker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InspectionWorker.Application.Strategies;

public class SimulatedInspectionStrategy : IInspectionStrategy
{
    private readonly ILogger<SimulatedInspectionStrategy> _logger;

    public SimulatedInspectionStrategy(ILogger<SimulatedInspectionStrategy> logger)
    {
        _logger = logger;
    }

    public InspectionResult Inspect(InspectionRequest request)
    {
        _logger.LogInformation("Inspecting product {ProductId} from request {RequestId}", 
            request.ProductId, request.RequestId);

        // Simulate inspection logic with weighted probabilities (using Random.Shared for thread safety)
        var randomValue = Random.Shared.NextDouble();
        var status = randomValue switch
        {
            < 0.70 => InspectionStatus.Pass,      // 70% pass
            < 0.90 => InspectionStatus.Defect,    // 20% defect
            _ => InspectionStatus.Anomaly         // 10% anomaly
        };

        var result = new InspectionResult
        {
            RequestId = request.RequestId,
            ProductId = request.ProductId,
            BatchId = request.BatchId,
            Status = status,
            InspectedAt = DateTime.UtcNow,
            ConfidenceScore = Random.Shared.NextDouble() * 0.3 + 0.7, // 0.7 to 1.0
            Metadata = new Dictionary<string, object>
            {
                { "InspectionDurationMs", Random.Shared.Next(50, 200) },
                { "InspectorVersion", "1.0.0" }
            }
        };

        if (status == InspectionStatus.Defect)
        {
            result.DefectType = GetRandomDefectType();
            result.Metadata["DefectSeverity"] = Random.Shared.Next(1, 10);
        }
        else if (status == InspectionStatus.Anomaly)
        {
            result.Metadata["AnomalyScore"] = Random.Shared.NextDouble();
        }

        _logger.LogInformation("Inspection completed for {RequestId}: Status={Status}, Confidence={Confidence:F2}", 
            request.RequestId, status, result.ConfidenceScore);

        return result;
    }

    private string GetRandomDefectType()
    {
        var defectTypes = new[] { "Scratch", "Dent", "Misalignment", "Discoloration", "Crack" };
        return defectTypes[Random.Shared.Next(defectTypes.Length)];
    }
}
