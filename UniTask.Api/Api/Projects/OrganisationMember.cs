using UniTask.Api.Users;

namespace UniTask.Api.Projects;

public class OrganisationMember
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public int UserId { get; set; }
    public string? Role { get; set; }

    // Navigation properties
    public Organisation Organisation { get; set; } = null!;
    public UniUser User { get; set; } = null!;
}
