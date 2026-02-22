using UniTask.Api.Shared;

namespace UniTask.Api.Projects;

public class Organisation
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public TaskProvider? Provider { get; set; }

    // Navigation properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<OrganisationMember> Members { get; set; } = new List<OrganisationMember>();
}
