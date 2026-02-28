using UniTask.Api.Users;

namespace UniTask.Api.Projects.Models;

public class OrganisationMember
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public Guid UserId { get; set; }
    public string? Role { get; set; }

    // Navigation properties
    public Organisation Organisation { get; set; } = null!;
    public UniUser User { get; set; } = null!;
}
