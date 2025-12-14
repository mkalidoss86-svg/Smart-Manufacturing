using Serilog;
using VisionFlow.Application;
using VisionFlow.Application.DTOs;
using VisionFlow.Application.Services;
using VisionFlow.Infrastructure;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ingestion-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting VisionFlow Ingestion API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add health checks
    builder.Services.AddHealthChecks();

    // Add application and infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Health check endpoint
    app.MapHealthChecks("/health");

    // Production quality events endpoint
    app.MapPost("/api/events", async (
        ProductionQualityEventDto eventDto,
        string lineId,
        QualityEventService eventService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            // Validate lineId
            if (string.IsNullOrWhiteSpace(lineId))
            {
                return Results.BadRequest(new { errors = new[] { "lineId is required" } });
            }

            var (isSuccess, errors) = await eventService.ProcessEventAsync(eventDto, lineId, cancellationToken);

            if (!isSuccess)
            {
                return Results.BadRequest(new { errors });
            }

            return Results.Accepted();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing quality event");
            return Results.Problem("An error occurred while processing the event");
        }
    })
    .WithName("IngestQualityEvent")
    .WithOpenApi()
    .Produces(StatusCodes.Status202Accepted)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
