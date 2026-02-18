using UniTask.Api.Projects;

namespace UniTask.Api.Tasks;

public class Status
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    
    // Foreign Keys
    public int? ProjectId { get; set; }

    // Navigation properties
    public Project? Project { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
