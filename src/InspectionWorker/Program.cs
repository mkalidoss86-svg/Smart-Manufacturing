using InspectionWorker;
using InspectionWorker.Application.Interfaces;
using InspectionWorker.Application.Services;
using InspectionWorker.Application.Strategies;
using InspectionWorker.Domain.Interfaces;
using InspectionWorker.Infrastructure.Configuration;
using InspectionWorker.Infrastructure.HealthChecks;
using InspectionWorker.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ settings
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

// Register Domain services
builder.Services.AddSingleton<IInspectionStrategy, SimulatedInspectionStrategy>();

// Register Application services
builder.Services.AddSingleton<IInspectionService, InspectionService>();

// Register Infrastructure services
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSingleton<RabbitMqConsumer>();

// Register Worker
builder.Services.AddHostedService<Worker>();

// Add Health Checks
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
builder.Services.AddHealthChecks()
    .AddCheck("RabbitMQ", new RabbitMqHealthCheck(
        rabbitMqSettings.HostName,
        rabbitMqSettings.Port,
        rabbitMqSettings.UserName,
        rabbitMqSettings.Password,
        rabbitMqSettings.VirtualHost));

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
    {
        Indented = false
    };
});

var host = builder.Build();

host.Run();
