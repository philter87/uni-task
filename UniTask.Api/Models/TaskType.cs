namespace UniTask.Api.Models;

public class TaskType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    // Foreign Keys
    public int? ProjectId { get; set; }

    // Navigation properties
    public Project? Project { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
