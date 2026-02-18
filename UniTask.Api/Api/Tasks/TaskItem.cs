using UniTask.Api.Projects;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks;

public class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    
    // Foreign Keys
    public int? ProjectId { get; set; }
    public int? TaskTypeId { get; set; }
    public int? StatusId { get; set; }
    public int? SprintId { get; set; }
    public int? ParentId { get; set; }
    
    public TaskPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public string? Source { get; set; } // "AzureDevOps" or "GitHub"
    public string? ExternalId { get; set; }
    
    // New fields
    public int? DurationMin { get; set; }
    public int? RemainingMin { get; set; }
    
    // Navigation properties
    public Project? Project { get; set; }
    public TaskType? TaskType { get; set; }
    public Status? Status { get; set; }
    public Sprint? Sprint { get; set; }
    public TaskItem? Parent { get; set; }
    public ICollection<TaskItem> Children { get; set; } = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Label> Labels { get; set; } = new List<Label>();
    public ICollection<TaskChange> Changes { get; set; } = new List<TaskChange>();
    public ICollection<TaskItemRelation> RelationsFrom { get; set; } = new List<TaskItemRelation>();
    public ICollection<TaskItemRelation> RelationsTo { get; set; } = new List<TaskItemRelation>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    
    // Deprecated - keeping for backwards compatibility during migration
    [Obsolete("Use StatusId and Status navigation property instead")]
    public TaskStatus OldStatus { get; set; }
}

public enum TaskStatus
{
    Todo,
    InProgress,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}
