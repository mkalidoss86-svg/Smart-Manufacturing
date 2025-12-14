using InspectionWorker.Domain.Entities;
using InspectionWorker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InspectionWorker.Application.Strategies;

public class SimulatedInspectionStrategy : IInspectionStrategy
{
    private readonly ILogger<SimulatedInspectionStrategy> _logger;
    private readonly Random _random = new();

    public SimulatedInspectionStrategy(ILogger<SimulatedInspectionStrategy> logger)
    {
        _logger = logger;
    }

    public InspectionResult Inspect(InspectionRequest request)
    {
        _logger.LogInformation("Inspecting product {ProductId} from request {RequestId}", 
            request.ProductId, request.RequestId);

        // Simulate inspection logic with weighted probabilities
        var randomValue = _random.NextDouble();
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
            ConfidenceScore = _random.NextDouble() * 0.3 + 0.7, // 0.7 to 1.0
            Metadata = new Dictionary<string, object>
            {
                { "InspectionDurationMs", _random.Next(50, 200) },
                { "InspectorVersion", "1.0.0" }
            }
        };

        if (status == InspectionStatus.Defect)
        {
            result.DefectType = GetRandomDefectType();
            result.Metadata["DefectSeverity"] = _random.Next(1, 10);
        }
        else if (status == InspectionStatus.Anomaly)
        {
            result.Metadata["AnomalyScore"] = _random.NextDouble();
        }

        _logger.LogInformation("Inspection completed for {RequestId}: Status={Status}, Confidence={Confidence:F2}", 
            request.RequestId, status, result.ConfidenceScore);

        return result;
    }

    private string GetRandomDefectType()
    {
        var defectTypes = new[] { "Scratch", "Dent", "Misalignment", "Discoloration", "Crack" };
        return defectTypes[_random.Next(defectTypes.Length)];
    }
}
