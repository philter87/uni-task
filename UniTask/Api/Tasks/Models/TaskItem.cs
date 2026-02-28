using UniTask.Api.Projects.Models;
using UniTask.Api.PullRequests;
using UniTask.Api.Shared;
using UniTask.Api.Users;

namespace UniTask.Api.Tasks.Models;

public class TaskItem
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    
    // Foreign Keys
    public Guid? ProjectId { get; set; }
    public Guid? TaskTypeId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? BoardId { get; set; }
    public Guid? ParentId { get; set; }
    
    public double Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public TaskProvider? Provider { get; set; }
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
    public ICollection<TaskItemRelation> RelationsFrom { get; set; } = new List<TaskItemRelation>();
    public ICollection<TaskItemRelation> RelationsTo { get; set; } = new List<TaskItemRelation>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
}
