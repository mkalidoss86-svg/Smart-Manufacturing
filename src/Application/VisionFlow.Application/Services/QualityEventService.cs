using VisionFlow.Application.DTOs;
using VisionFlow.Application.Interfaces;
using VisionFlow.Domain.Events;
using FluentValidation;

namespace VisionFlow.Application.Services;

public class QualityEventService
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<ProductionQualityEventDto> _validator;

    public QualityEventService(IEventPublisher eventPublisher, IValidator<ProductionQualityEventDto> validator)
    {
        _eventPublisher = eventPublisher;
        _validator = validator;
    }

    public async Task<(bool IsSuccess, IEnumerable<string> Errors)> ProcessEventAsync(
        ProductionQualityEventDto eventDto, 
        string lineId,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(eventDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return (false, validationResult.Errors.Select(e => e.ErrorMessage));
        }

        // Enrich event with ID, timestamp, and lineId
        var domainEvent = new ProductionQualityEvent
        {
            EventId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            LineId = lineId,
            ProductId = eventDto.ProductId,
            BatchId = eventDto.BatchId,
            StationId = eventDto.StationId,
            QualityMetrics = eventDto.QualityMetrics,
            Status = eventDto.Status,
            AdditionalData = eventDto.AdditionalData
        };

        // Publish to RabbitMQ
        await _eventPublisher.PublishAsync(domainEvent, cancellationToken);

        return (true, Enumerable.Empty<string>());
    }
}
