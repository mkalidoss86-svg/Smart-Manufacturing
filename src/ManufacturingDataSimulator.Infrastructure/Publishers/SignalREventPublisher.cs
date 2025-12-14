using ManufacturingDataSimulator.Application.Services;
using ManufacturingDataSimulator.Domain.Entities;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace ManufacturingDataSimulator.Infrastructure.Publishers;

public class SignalREventPublisher : IManufacturingEventPublisher
{
    private readonly ILogger<SignalREventPublisher> _logger;
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    public SignalREventPublisher(
        string hubUrl,
        ILogger<SignalREventPublisher> logger)
    {
        _hubUrl = hubUrl;
        _logger = logger;
    }

    private async Task EnsureConnectedAsync()
    {
        if (_hubConnection == null)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                _logger.LogWarning("SignalR connection closed: {Error}", error?.Message);
                await Task.Delay(5000);
            };

            _hubConnection.Reconnecting += error =>
            {
                _logger.LogWarning("SignalR reconnecting: {Error}", error?.Message);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                _logger.LogInformation("SignalR reconnected: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };
        }

        if (_hubConnection.State != HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("SignalR connected to {HubUrl}", _hubUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to SignalR hub");
            }
        }
    }

    public async Task PublishEventAsync(ManufacturingEvent evt)
    {
        try
        {
            await EnsureConnectedAsync();

            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("PublishManufacturingEvent", new
                {
                    evt.Id,
                    evt.ProductionLine,
                    evt.Timestamp,
                    evt.BatchId,
                    evt.ProductId,
                    DefectType = evt.DefectType.ToString(),
                    Severity = evt.Severity.ToString(),
                    Status = evt.Status.ToString(),
                    evt.ConfidenceScore,
                    evt.Metadata
                });

                _logger.LogDebug("Published event {Id} to SignalR", evt.Id);
            }
            else
            {
                _logger.LogWarning("SignalR not connected, event {Id} not published", evt.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {Id} to SignalR", evt.Id);
        }
    }
}
