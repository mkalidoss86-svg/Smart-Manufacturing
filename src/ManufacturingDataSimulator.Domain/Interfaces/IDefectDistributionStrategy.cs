using ManufacturingDataSimulator.Domain.Enums;

namespace ManufacturingDataSimulator.Domain.Interfaces;

public interface IDefectDistributionStrategy
{
    (DefectType Type, DefectSeverity Severity, QualityStatus Status) GenerateDefect(double randomValue);
    double DefectProbability { get; }
}
