using UniTask.Api.Shared;
using UniTask.Api.Tasks;

namespace UniTask.Api.Projects;

public class Project
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? OrganisationId { get; set; }
    public TaskProvider? Provider { get; set; }

    // Navigation properties
    public Organisation? Organisation { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<TaskType> TaskTypes { get; set; } = new List<TaskType>();
}
