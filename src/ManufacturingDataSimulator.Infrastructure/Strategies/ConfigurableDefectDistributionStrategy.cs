using ManufacturingDataSimulator.Domain.Enums;
using ManufacturingDataSimulator.Domain.Interfaces;

namespace ManufacturingDataSimulator.Infrastructure.Strategies;

public class ConfigurableDefectDistributionStrategy : IDefectDistributionStrategy
{
    private readonly double _defectProbability;
    private static readonly ThreadLocal<Random> _threadRandom = new(() => new Random(Guid.NewGuid().GetHashCode()));

    public ConfigurableDefectDistributionStrategy(double defectPercentage)
    {
        _defectProbability = Math.Clamp(defectPercentage / 100.0, 0.0, 1.0);
    }

    public double DefectProbability => _defectProbability;

    public (DefectType Type, DefectSeverity Severity, QualityStatus Status) GenerateDefect(double randomValue)
    {
        if (randomValue >= _defectProbability)
        {
            return (DefectType.None, DefectSeverity.None, QualityStatus.Pass);
        }

        var defectTypes = Enum.GetValues<DefectType>().Where(d => d != DefectType.None).ToArray();
        var defectType = defectTypes[_threadRandom.Value!.Next(defectTypes.Length)];

        var severityRoll = _threadRandom.Value!.NextDouble();
        DefectSeverity severity;
        QualityStatus status;

        if (severityRoll < 0.50)
        {
            severity = DefectSeverity.Low;
            status = QualityStatus.Warning;
        }
        else if (severityRoll < 0.80)
        {
            severity = DefectSeverity.Medium;
            status = QualityStatus.Warning;
        }
        else if (severityRoll < 0.95)
        {
            severity = DefectSeverity.High;
            status = QualityStatus.Fail;
        }
        else
        {
            severity = DefectSeverity.Critical;
            status = QualityStatus.Fail;
        }

        return (defectType, severity, status);
    }
}
