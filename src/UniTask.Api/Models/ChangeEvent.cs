namespace UniTask.Api.Models;

public class ChangeEvent
{
    public int Id { get; set; }
    public required string EventId { get; set; } // ULID or UUIDv7
    public int? ProjectId { get; set; }
    public required string EntityType { get; set; } // "Task", "Comment", "Label", etc.
    public int EntityId { get; set; }
    public required string Operation { get; set; } // "Created", "Updated", "Deleted"
    public DateTime OccurredAt { get; set; }
    public string? ActorUserId { get; set; }
    public int Version { get; set; }
    public string? Payload { get; set; } // JSON string - snapshot for Created, patch for Updated, minimal for Deleted
    
    // Navigation properties
    public Project? Project { get; set; }
}

public static class ChangeEventOperation
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string Deleted = "Deleted";
}

public static class ChangeEventEntityType
{
    public const string Task = "Task";
    public const string Comment = "Comment";
    public const string Label = "Label";
    public const string Project = "Project";
    public const string Status = "Status";
    public const string Sprint = "Sprint";
    public const string TaskType = "TaskType";
}
