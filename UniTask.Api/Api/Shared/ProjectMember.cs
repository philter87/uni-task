using UniTask.Api.Projects;
using UniTask.Api.Users;

namespace UniTask.Api.Shared;

public class ProjectMember
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
