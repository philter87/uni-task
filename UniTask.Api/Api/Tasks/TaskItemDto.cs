using UniTask.Api.Projects;
using UniTask.Api.Shared;
using System.ComponentModel.DataAnnotations;

namespace UniTask.Api.Tasks;

public class TaskItemDto
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    
    public Guid? ProjectId { get; set; }
    public Guid? TaskTypeId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? BoardId { get; set; }
    public Guid? ParentId { get; set; }
    
    [Range(0, 10)]
    public double Priority { get; set; } = 5.0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public TaskProvider? Provider { get; set; }
    
    public double? DurationHours { get; set; }
    public double? DurationRemainingHours { get; set; }
    
    public ProjectDto? Project { get; set; }
    public TaskTypeDto? TaskType { get; set; }
    public StatusDto? Status { get; set; }
    public BoardDto? Board { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
    public List<LabelDto> Labels { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();
}
