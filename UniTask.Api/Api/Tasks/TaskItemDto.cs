using UniTask.Api.Projects;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks;

public class TaskItemDto
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    
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
    public string? Source { get; set; }
    
    public double? DurationHours { get; set; }
    public double? DurationRemainingHours { get; set; }
    
    public ProjectDto? Project { get; set; }
    public TaskTypeDto? TaskType { get; set; }
    public StatusDto? Status { get; set; }
    public BoardDto? Board { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
    public List<LabelDto> Labels { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();
}
