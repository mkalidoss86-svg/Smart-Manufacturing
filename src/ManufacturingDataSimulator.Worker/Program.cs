using ManufacturingDataSimulator.Application.Configuration;
using ManufacturingDataSimulator.Application.Services;
using ManufacturingDataSimulator.Domain.Interfaces;
using ManufacturingDataSimulator.Infrastructure.Factories;
using ManufacturingDataSimulator.Infrastructure.Persistence;
using ManufacturingDataSimulator.Infrastructure.Publishers;
using ManufacturingDataSimulator.Infrastructure.Strategies;
using ManufacturingDataSimulator.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.Configure<SimulatorSettings>(
    builder.Configuration.GetSection("Simulator"));

var dbProvider = builder.Configuration.GetValue<string>("Database:Provider") ?? "sqlite";
var connectionString = builder.Configuration.GetConnectionString("ManufacturingDb") 
    ?? "Data Source=manufacturing.db";

if (dbProvider.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<ManufacturingDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<ManufacturingDbContext>(options =>
        options.UseSqlite(connectionString));
}

var defectPercentage = builder.Configuration.GetValue<double>("Simulator:DefectPercentage", 10.0);
builder.Services.AddSingleton<IDefectDistributionStrategy>(
    new ConfigurableDefectDistributionStrategy(defectPercentage));

builder.Services.AddScoped<IEventFactory, ManufacturingEventFactory>();
builder.Services.AddScoped<IManufacturingEventRepository, ManufacturingEventRepository>();

var notificationHubUrl = builder.Configuration.GetValue<string>("NotificationService:HubUrl") 
    ?? "http://localhost:8080/hubs/inspection";
builder.Services.AddSingleton<IManufacturingEventPublisher>(sp =>
    new SignalREventPublisher(
        notificationHubUrl,
        sp.GetRequiredService<ILogger<SignalREventPublisher>>()));

builder.Services.AddScoped<SimulatorService>();

builder.Services.AddHostedService<SimulatorWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ManufacturingDbContext>();
    db.Database.EnsureCreated();
    Log.Information("Database initialized");
}

Log.Information("Manufacturing Data Simulator Worker starting...");
host.Run();
