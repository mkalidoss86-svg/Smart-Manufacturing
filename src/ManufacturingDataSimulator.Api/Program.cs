using ManufacturingDataSimulator.Application.Configuration;
using ManufacturingDataSimulator.Application.DTOs;
using ManufacturingDataSimulator.Application.Services;
using ManufacturingDataSimulator.Domain.Interfaces;
using ManufacturingDataSimulator.Infrastructure.Factories;
using ManufacturingDataSimulator.Infrastructure.Persistence;
using ManufacturingDataSimulator.Infrastructure.Publishers;
using ManufacturingDataSimulator.Infrastructure.Strategies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Manufacturing Data Simulator API", Version = "v1" });
});

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

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ManufacturingDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ManufacturingDbContext>();
    db.Database.EnsureCreated();
    Log.Information("Database initialized");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.MapHealthChecks("/health");

app.MapPost("/api/events", async (
    [FromServices] SimulatorService service,
    [FromServices] ILogger<Program> logger) =>
{
    try
    {
        var evt = await service.GenerateAndPersistEventAsync();
        
        var response = new ManufacturingEventDto(
            evt.Id,
            evt.ProductionLine,
            evt.Timestamp,
            evt.BatchId,
            evt.ProductId,
            evt.DefectType.ToString(),
            evt.Severity.ToString(),
            evt.Status.ToString(),
            evt.ConfidenceScore,
            evt.Metadata
        );

        return Results.Created($"/api/events/{evt.Id}", response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating event");
        return Results.Problem("Failed to generate event", statusCode: 500);
    }
})
.WithName("GenerateEvent")
.WithTags("Events");

app.MapPost("/api/events/batch", async (
    [FromQuery] int count,
    [FromServices] SimulatorService service,
    [FromServices] ILogger<Program> logger) =>
{
    try
    {
        if (count < 1 || count > 100)
        {
            return Results.BadRequest("Count must be between 1 and 100");
        }

        var events = await service.GenerateMultipleEventsAsync(count);
        
        var responses = events.Select(evt => new ManufacturingEventDto(
            evt.Id,
            evt.ProductionLine,
            evt.Timestamp,
            evt.BatchId,
            evt.ProductId,
            evt.DefectType.ToString(),
            evt.Severity.ToString(),
            evt.Status.ToString(),
            evt.ConfidenceScore,
            evt.Metadata
        ));

        return Results.Ok(new { Count = events.Count(), Events = responses });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating batch events");
        return Results.Problem("Failed to generate batch events", statusCode: 500);
    }
})
.WithName("GenerateBatchEvents")
.WithTags("Events");

app.MapGet("/api/events/{id:guid}", async (
    [FromRoute] Guid id,
    [FromServices] IManufacturingEventRepository repository,
    [FromServices] ILogger<Program> logger) =>
{
    try
    {
        var evt = await repository.GetByIdAsync(id);

        if (evt == null)
        {
            return Results.NotFound(new { message = $"Event with ID {id} not found" });
        }

        var response = new ManufacturingEventDto(
            evt.Id,
            evt.ProductionLine,
            evt.Timestamp,
            evt.BatchId,
            evt.ProductId,
            evt.DefectType.ToString(),
            evt.Severity.ToString(),
            evt.Status.ToString(),
            evt.ConfidenceScore,
            evt.Metadata
        );

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving event {Id}", id);
        return Results.Problem("Failed to retrieve event", statusCode: 500);
    }
})
.WithName("GetEventById")
.WithTags("Events");

app.MapGet("/api/events", async (
    [FromServices] IManufacturingEventRepository repository,
    [FromServices] ILogger<Program> logger,
    [FromQuery] string? productionLine = null,
    [FromQuery] string? status = null,
    [FromQuery] DateTime? startTime = null,
    [FromQuery] DateTime? endTime = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50) =>
{
    try
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        var (events, totalCount) = await repository.QueryAsync(
            productionLine, status, startTime, endTime, page, pageSize);

        var responseItems = events.Select(evt => new ManufacturingEventDto(
            evt.Id,
            evt.ProductionLine,
            evt.Timestamp,
            evt.BatchId,
            evt.ProductId,
            evt.DefectType.ToString(),
            evt.Severity.ToString(),
            evt.Status.ToString(),
            evt.ConfidenceScore,
            evt.Metadata
        ));

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var response = new PagedResponse<ManufacturingEventDto>(
            responseItems,
            totalCount,
            page,
            pageSize,
            totalPages
        );

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error querying events");
        return Results.Problem("Failed to query events", statusCode: 500);
    }
})
.WithName("QueryEvents")
.WithTags("Events");

app.MapGet("/api/events/recent", async (
    [FromServices] IManufacturingEventRepository repository,
    [FromServices] ILogger<Program> logger,
    [FromQuery] int count = 100) =>
{
    try
    {
        if (count < 1 || count > 500) count = 100;

        var events = await repository.GetRecentEventsAsync(count);
        
        var responses = events.Select(evt => new ManufacturingEventDto(
            evt.Id,
            evt.ProductionLine,
            evt.Timestamp,
            evt.BatchId,
            evt.ProductId,
            evt.DefectType.ToString(),
            evt.Severity.ToString(),
            evt.Status.ToString(),
            evt.ConfidenceScore,
            evt.Metadata
        ));

        return Results.Ok(responses);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving recent events");
        return Results.Problem("Failed to retrieve recent events", statusCode: 500);
    }
})
.WithName("GetRecentEvents")
.WithTags("Events");

app.MapGet("/api/stats/production-lines", async (
    [FromServices] IManufacturingEventRepository repository,
    [FromServices] ILogger<Program> logger) =>
{
    try
    {
        var stats = await repository.GetProductionLineStatsAsync();
        return Results.Ok(stats);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving production line stats");
        return Results.Problem("Failed to retrieve stats", statusCode: 500);
    }
})
.WithName("GetProductionLineStats")
.WithTags("Statistics");

Log.Information("Manufacturing Data Simulator API starting...");
app.Run();
