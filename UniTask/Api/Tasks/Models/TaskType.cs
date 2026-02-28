using UniTask.Api.Projects.Models;

namespace UniTask.Api.Tasks.Models;

public class TaskType
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    // Foreign Keys
    public Guid? ProjectId { get; set; }

    // Navigation properties
    public Project? Project { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<Status> Statuses { get; set; } = new List<Status>();
}
