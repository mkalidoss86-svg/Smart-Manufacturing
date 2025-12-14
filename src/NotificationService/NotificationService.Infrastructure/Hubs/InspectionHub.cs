using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Hubs;

public class InspectionHub : Hub
{
    private static readonly Dictionary<string, long> _clientSequenceNumbers = new();

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        _clientSequenceNumbers[connectionId] = 0;
        
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("Connected", new { ConnectionId = connectionId, Timestamp = DateTime.UtcNow });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        _clientSequenceNumbers.Remove(connectionId);
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<long> GetLastSequenceNumber()
    {
        var connectionId = Context.ConnectionId;
        return _clientSequenceNumbers.TryGetValue(connectionId, out var sequenceNumber) ? sequenceNumber : 0;
    }

    public Task UpdateSequenceNumber(long sequenceNumber)
    {
        var connectionId = Context.ConnectionId;
        _clientSequenceNumbers[connectionId] = sequenceNumber;
        return Task.CompletedTask;
    }

    public async Task SubscribeToInspections()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "InspectionUpdates");
        await Clients.Caller.SendAsync("Subscribed", "InspectionUpdates");
    }

    public async Task UnsubscribeFromInspections()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "InspectionUpdates");
        await Clients.Caller.SendAsync("Unsubscribed", "InspectionUpdates");
    }
}
