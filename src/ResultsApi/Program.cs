using Microsoft.AspNetCore.Mvc;
using ResultsApi.Application.DTOs;
using ResultsApi.Application.Interfaces;
using ResultsApi.Domain;
using ResultsApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "VisionFlow Results API", Version = "v1" });
});

// Register repository (in-memory)
builder.Services.AddSingleton<IInspectionResultRepository, InMemoryInspectionResultRepository>();

// Add health checks
builder.Services.AddHealthChecks();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddJsonConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Health endpoint
app.MapHealthChecks("/health");

// POST: Create inspection result
app.MapPost("/api/results", async (
    [FromBody] CreateInspectionResultRequest request,
    [FromServices] IInspectionResultRepository repository,
    [FromServices] ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Creating inspection result for line {LineId} with status {Status}", 
            request.LineId, request.Status);

        var result = new InspectionResult
        {
            Id = Guid.NewGuid(),
            LineId = request.LineId,
            Status = request.Status,
            Timestamp = request.Timestamp,
            ProductId = request.ProductId,
            DefectType = request.DefectType,
            ConfidenceScore = request.ConfidenceScore,
            Metadata = request.Metadata
        };

        var created = await repository.AddAsync(result);

        var response = new InspectionResultResponse(
            created.Id,
            created.LineId,
            created.Status,
            created.Timestamp,
            created.ProductId,
            created.DefectType,
            created.ConfidenceScore,
            created.Metadata
        );

        logger.LogInformation("Successfully created inspection result {Id}", created.Id);

        return Results.Created($"/api/results/{created.Id}", response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating inspection result");
        return Results.Problem("Failed to create inspection result", statusCode: 500);
    }
})
.WithName("CreateInspectionResult")
.WithOpenApi()
.WithTags("Inspection Results");

// GET: Get inspection result by ID
app.MapGet("/api/results/{id:guid}", async (
    [FromRoute] Guid id,
    [FromServices] IInspectionResultRepository repository,
    [FromServices] ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Retrieving inspection result {Id}", id);

        var result = await repository.GetByIdAsync(id);

        if (result == null)
        {
            logger.LogWarning("Inspection result {Id} not found", id);
            return Results.NotFound(new { message = $"Inspection result with ID {id} not found" });
        }

        var response = new InspectionResultResponse(
            result.Id,
            result.LineId,
            result.Status,
            result.Timestamp,
            result.ProductId,
            result.DefectType,
            result.ConfidenceScore,
            result.Metadata
        );

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving inspection result {Id}", id);
        return Results.Problem("Failed to retrieve inspection result", statusCode: 500);
    }
})
.WithName("GetInspectionResultById")
.WithOpenApi()
.WithTags("Inspection Results");

// GET: Query inspection results with filters and pagination
app.MapGet("/api/results", async (
    [FromServices] IInspectionResultRepository repository,
    [FromServices] ILogger<Program> logger,
    [FromQuery] string? lineId = null,
    [FromQuery] string? status = null,
    [FromQuery] DateTime? startTime = null,
    [FromQuery] DateTime? endTime = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50) =>
{
    try
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        logger.LogInformation(
            "Querying inspection results with filters - LineId: {LineId}, Status: {Status}, StartTime: {StartTime}, EndTime: {EndTime}, Page: {Page}, PageSize: {PageSize}",
            lineId ?? "none", status ?? "none", startTime?.ToString() ?? "none", endTime?.ToString() ?? "none", page, pageSize);

        var (results, totalCount) = await repository.QueryAsync(
            lineId, status, startTime, endTime, page, pageSize);

        var responseItems = results.Select(r => new InspectionResultResponse(
            r.Id,
            r.LineId,
            r.Status,
            r.Timestamp,
            r.ProductId,
            r.DefectType,
            r.ConfidenceScore,
            r.Metadata
        )).ToList();

        var response = new PagedResponse<InspectionResultResponse>(
            responseItems,
            totalCount,
            page,
            pageSize
        );

        logger.LogInformation("Query returned {Count} results out of {Total}", responseItems.Count, totalCount);

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error querying inspection results");
        return Results.Problem("Failed to query inspection results", statusCode: 500);
    }
})
.WithName("QueryInspectionResults")
.WithOpenApi()
.WithTags("Inspection Results");

app.Run();
