using UniTask.Api.Projects;
using UniTask.Api.PullRequests;
using UniTask.Api.Shared;
using UniTask.Api.Users;

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
    public int? BoardId { get; set; }
    public int? ParentId { get; set; }
    
    public double Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public int? AssignedToUserId { get; set; }
    public TaskSource? Source { get; set; }
    public string? ExternalId { get; set; }
    
    // New fields
    public double? DurationHours { get; set; }
    public double? DurationRemainingHours { get; set; }
    
    // Navigation properties
    public Project? Project { get; set; }
    public TaskType? TaskType { get; set; }
    public Status? Status { get; set; }
    public Board? Board { get; set; }
    public TaskItem? Parent { get; set; }
    public UniUser? AssignedToUser { get; set; }
    public ICollection<TaskItem> Children { get; set; } = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Label> Labels { get; set; } = new List<Label>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<TaskChange> Changes { get; set; } = new List<TaskChange>();
    public ICollection<TaskItemRelation> RelationsFrom { get; set; } = new List<TaskItemRelation>();
    public ICollection<TaskItemRelation> RelationsTo { get; set; } = new List<TaskItemRelation>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
}

public enum TaskSource
{
    Local,
    AzureDevOps,
    GitHub
}
