using UniTask.Api.Models;

namespace UniTask.Api.Services;

public interface IChangeEventService
{
    /// <summary>
    /// Creates a change event for an entity operation
    /// </summary>
    Task<ChangeEvent> CreateChangeEventAsync(
        int? projectId,
        string entityType,
        int entityId,
        string operation,
        string? actorUserId);

    /// <summary>
    /// Gets change events for a specific project
    /// </summary>
    Task<IEnumerable<ChangeEvent>> GetChangeEventsAsync(int? projectId = null, int? sinceVersion = null);

    /// <summary>
    /// Gets change events for a specific entity
    /// </summary>
    Task<IEnumerable<ChangeEvent>> GetEntityChangeEventsAsync(string entityType, int entityId);
}
