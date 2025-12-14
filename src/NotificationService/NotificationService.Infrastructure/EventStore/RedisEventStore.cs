using Microsoft.Extensions.Caching.Distributed;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace NotificationService.Infrastructure.EventStore;

public class RedisEventStore : IEventStore
{
    private readonly IDistributedCache _cache;
    private const string EventKeyPrefix = "event:";
    private const string SequenceKey = "sequence:latest";

    public RedisEventStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task StoreEventAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken = default)
    {
        var key = $"{EventKeyPrefix}{notificationEvent.SequenceNumber}";
        var json = JsonSerializer.Serialize(notificationEvent);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // Keep events for 24 hours
        };

        await _cache.SetStringAsync(key, json, options, cancellationToken);
        
        // Update latest sequence number
        await _cache.SetStringAsync(SequenceKey, notificationEvent.SequenceNumber.ToString(), cancellationToken);
    }

    public async Task<IEnumerable<NotificationEvent>> GetMissedEventsAsync(long lastSequenceNumber, int maxCount = 100, CancellationToken cancellationToken = default)
    {
        var events = new List<NotificationEvent>();
        var latestSequence = await GetLatestSequenceNumberAsync(cancellationToken);

        var count = 0;
        for (var i = lastSequenceNumber + 1; i <= latestSequence && count < maxCount; i++)
        {
            var key = $"{EventKeyPrefix}{i}";
            var json = await _cache.GetStringAsync(key, cancellationToken);
            
            if (!string.IsNullOrEmpty(json))
            {
                var notificationEvent = JsonSerializer.Deserialize<NotificationEvent>(json);
                if (notificationEvent != null)
                {
                    events.Add(notificationEvent);
                    count++;
                }
            }
        }

        return events;
    }

    public async Task<long> GetLatestSequenceNumberAsync(CancellationToken cancellationToken = default)
    {
        var sequenceStr = await _cache.GetStringAsync(SequenceKey, cancellationToken);
        return string.IsNullOrEmpty(sequenceStr) ? 0 : long.Parse(sequenceStr);
    }
}
