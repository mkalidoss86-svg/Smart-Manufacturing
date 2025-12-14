using InspectionWorker.Application.Interfaces;
using InspectionWorker.Domain.Entities;
using InspectionWorker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InspectionWorker.Application.Services;

public class InspectionService : IInspectionService
{
    private readonly IInspectionStrategy _inspectionStrategy;
    private readonly ILogger<InspectionService> _logger;
    private readonly HashSet<string> _processedRequests = new();
    private readonly object _lock = new();

    public InspectionService(
        IInspectionStrategy inspectionStrategy,
        ILogger<InspectionService> logger)
    {
        _inspectionStrategy = inspectionStrategy;
        _logger = logger;
    }

    public Task<InspectionResult> ProcessInspectionAsync(InspectionRequest request, CancellationToken cancellationToken = default)
    {
        // Idempotency check
        lock (_lock)
        {
            if (_processedRequests.Contains(request.RequestId))
            {
                _logger.LogWarning("Duplicate request detected: {RequestId}. Skipping processing.", request.RequestId);
                throw new InvalidOperationException($"Request {request.RequestId} has already been processed");
            }
            _processedRequests.Add(request.RequestId);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Processing inspection request {RequestId}", request.RequestId);
            var result = _inspectionStrategy.Inspect(request);
            
            return Task.FromResult(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Inspection processing cancelled for request {RequestId}", request.RequestId);
            
            // Remove from processed set if cancelled
            lock (_lock)
            {
                _processedRequests.Remove(request.RequestId);
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inspection request {RequestId}", request.RequestId);
            
            // Remove from processed set on error to allow retry
            lock (_lock)
            {
                _processedRequests.Remove(request.RequestId);
            }
            throw;
        }
    }
}
