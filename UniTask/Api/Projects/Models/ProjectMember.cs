using UniTask.Api.Users;

namespace UniTask.Api.Projects.Models;

public class ProjectMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public UniUser User { get; set; } = null!;
}
