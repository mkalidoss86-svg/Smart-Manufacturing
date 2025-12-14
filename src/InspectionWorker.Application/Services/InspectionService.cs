using InspectionWorker.Application.Interfaces;
using InspectionWorker.Domain.Entities;
using InspectionWorker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InspectionWorker.Application.Services;

public class InspectionService : IInspectionService
{
    private readonly IInspectionStrategy _inspectionStrategy;
    private readonly ILogger<InspectionService> _logger;
    private readonly Dictionary<string, DateTime> _processedRequests = new();
    private readonly object _lock = new();
    private readonly TimeSpan _idempotencyWindow = TimeSpan.FromHours(1); // Track requests for 1 hour

    public InspectionService(
        IInspectionStrategy inspectionStrategy,
        ILogger<InspectionService> logger)
    {
        _inspectionStrategy = inspectionStrategy;
        _logger = logger;
    }

    public Task<InspectionResult> ProcessInspectionAsync(InspectionRequest request, CancellationToken cancellationToken = default)
    {
        // Idempotency check with time-based cleanup
        lock (_lock)
        {
            // Clean up old entries
            CleanupExpiredRequests();

            if (_processedRequests.ContainsKey(request.RequestId))
            {
                _logger.LogWarning("Duplicate request detected: {RequestId}. Skipping processing.", request.RequestId);
                throw new InvalidOperationException($"Request {request.RequestId} has already been processed");
            }
            _processedRequests[request.RequestId] = DateTime.UtcNow;
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

    private void CleanupExpiredRequests()
    {
        var cutoffTime = DateTime.UtcNow - _idempotencyWindow;
        var expiredKeys = _processedRequests
            .Where(kvp => kvp.Value < cutoffTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _processedRequests.Remove(key);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired request IDs from idempotency cache", expiredKeys.Count);
        }
    }
}
