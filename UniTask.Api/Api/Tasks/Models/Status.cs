using UniTask.Api.Projects;

namespace UniTask.Api.Tasks;

public class Status
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    
    // Foreign Keys
    public Guid? TaskTypeId { get; set; }

    // Navigation properties
    public TaskType? TaskType { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
