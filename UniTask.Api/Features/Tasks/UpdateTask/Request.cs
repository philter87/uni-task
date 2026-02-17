namespace UniTask.Api.Features.Tasks.UpdateTask;

public class UpdateTaskCommand
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    
    public int? ProjectId { get; set; }
    public int? TaskTypeId { get; set; }
    public int? StatusId { get; set; }
    public int? SprintId { get; set; }
    
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public string? Source { get; set; }
    public string? ExternalId { get; set; }
    
    public int? DurationMin { get; set; }
    public int? RemainingMin { get; set; }
}
