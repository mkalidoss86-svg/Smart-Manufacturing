namespace ManufacturingDataSimulator.Application.Configuration;

public class SimulatorSettings
{
    public int EventRatePerSecond { get; set; } = 2;
    public double DefectPercentage { get; set; } = 10.0;
    public bool EnableBurstMode { get; set; } = false;
    public int BurstIntervalSeconds { get; set; } = 60;
    public int BurstMultiplier { get; set; } = 5;
    public int? RandomSeed { get; set; }
    public string[] ProductionLines { get; set; } = new[] { "Line-A", "Line-B", "Line-C" };
    public bool ContinuousMode { get; set; } = true;
}
