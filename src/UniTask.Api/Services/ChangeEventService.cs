using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Data;
using UniTask.Api.Models;

namespace UniTask.Api.Services;

public class ChangeEventService : IChangeEventService
{
    private readonly TaskDbContext _context;

    public ChangeEventService(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<ChangeEvent> CreateChangeEventAsync(
        int? projectId,
        string entityType,
        int entityId,
        string operation,
        string? actorUserId,
        object? payload = null)
    {
        var version = await GetNextVersionAsync();
        var eventId = GenerateEventId();
        
        var changeEvent = new ChangeEvent
        {
            EventId = eventId,
            ProjectId = projectId,
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation,
            OccurredAt = DateTime.UtcNow,
            ActorUserId = actorUserId,
            Version = version,
            Payload = payload != null ? JsonSerializer.Serialize(payload) : null
        };

        _context.ChangeEvents.Add(changeEvent);
        await _context.SaveChangesAsync();

        return changeEvent;
    }

    public async Task<IEnumerable<ChangeEvent>> GetChangeEventsAsync(int? projectId = null, int? sinceVersion = null)
    {
        var query = _context.ChangeEvents.AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(e => e.ProjectId == projectId.Value);
        }

        if (sinceVersion.HasValue)
        {
            query = query.Where(e => e.Version > sinceVersion.Value);
        }

        return await query
            .OrderBy(e => e.Version)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChangeEvent>> GetEntityChangeEventsAsync(string entityType, int entityId)
    {
        return await _context.ChangeEvents
            .Where(e => e.EntityType == entityType && e.EntityId == entityId)
            .OrderBy(e => e.Version)
            .ToListAsync();
    }

    private async Task<int> GetNextVersionAsync()
    {
        // Use database to determine next version to avoid issues in multi-instance deployments
        var maxVersion = await _context.ChangeEvents
            .MaxAsync(e => (int?)e.Version) ?? 0;
        return maxVersion + 1;
    }

    private static string GenerateEventId()
    {
        // Using a simple GUID-based approach for now
        // In production, consider using ULID or UUIDv7 for better sortability
        return Guid.NewGuid().ToString();
    }
}
