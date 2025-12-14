using ManufacturingDataSimulator.Domain.Entities;
using ManufacturingDataSimulator.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManufacturingDataSimulator.Application.Services;

public class SimulatorService
{
    private readonly IEventFactory _eventFactory;
    private readonly IManufacturingEventRepository _repository;
    private readonly IManufacturingEventPublisher _publisher;
    private readonly ILogger<SimulatorService> _logger;

    public SimulatorService(
        IEventFactory eventFactory,
        IManufacturingEventRepository repository,
        IManufacturingEventPublisher publisher,
        ILogger<SimulatorService> logger)
    {
        _eventFactory = eventFactory;
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<ManufacturingEvent> GenerateAndPersistEventAsync(int? seed = null)
    {
        try
        {
            var evt = _eventFactory.CreateEvent(seed);
            
            _logger.LogInformation(
                "Generated event for line {Line}, Status: {Status}, Defect: {Defect}",
                evt.ProductionLine, evt.Status, evt.DefectType);

            var savedEvent = await _repository.AddAsync(evt);
            
            await _publisher.PublishEventAsync(savedEvent);
            
            _logger.LogInformation("Event {Id} persisted and published", savedEvent.Id);
            
            return savedEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating and persisting event");
            throw;
        }
    }

    public async Task<IEnumerable<ManufacturingEvent>> GenerateMultipleEventsAsync(int count, int? seed = null)
    {
        var events = new List<ManufacturingEvent>();
        
        for (int i = 0; i < count; i++)
        {
            var evt = await GenerateAndPersistEventAsync(seed.HasValue ? seed.Value + i : null);
            events.Add(evt);
        }

        _logger.LogInformation("Generated and persisted {Count} events", count);
        
        return events;
    }
}
