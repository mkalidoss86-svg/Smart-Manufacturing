using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Services;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionNotificationService _notificationService;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(
        IInspectionNotificationService notificationService,
        ILogger<InspectionsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PublishInspectionResult([FromBody] InspectionUpdateDto inspectionUpdate)
    {
        try
        {
            _logger.LogInformation("Received inspection result: {InspectionId}", inspectionUpdate.Id);
            await _notificationService.ProcessInspectionResultAsync(inspectionUpdate);
            return Ok(new { Message = "Inspection result published successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing inspection result");
            return StatusCode(500, new { Error = "Failed to publish inspection result" });
        }
    }

    [HttpGet("missed-events")]
    public async Task<IActionResult> GetMissedEvents([FromQuery] long lastSequenceNumber, [FromQuery] int maxCount = 100)
    {
        try
        {
            _logger.LogInformation("Retrieving missed events from sequence: {SequenceNumber}", lastSequenceNumber);
            var events = await _notificationService.GetMissedEventsAsync(lastSequenceNumber, maxCount);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving missed events");
            return StatusCode(500, new { Error = "Failed to retrieve missed events" });
        }
    }

    [HttpGet("latest-sequence")]
    public async Task<IActionResult> GetLatestSequenceNumber()
    {
        try
        {
            var sequenceNumber = await _notificationService.GetLatestSequenceNumberAsync();
            return Ok(new { SequenceNumber = sequenceNumber });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest sequence number");
            return StatusCode(500, new { Error = "Failed to retrieve latest sequence number" });
        }
    }
}
