using UniTask.Api.Shared;
using UniTask.Api.Tasks;

namespace UniTask.Api.Projects;

public class Project
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<Status> Statuses { get; set; } = new List<Status>();
    public ICollection<TaskType> TaskTypes { get; set; } = new List<TaskType>();
}
