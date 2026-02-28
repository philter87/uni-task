using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Models;

public class Organisation
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public TaskProvider? Provider { get; set; }

    // Navigation properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<OrganisationMember> Members { get; set; } = new List<OrganisationMember>();
    public ICollection<TaskProviderAuth> Auths { get; set; } = new List<TaskProviderAuth>();
}
