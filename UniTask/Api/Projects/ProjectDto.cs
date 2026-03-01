using UniTask.Api.Shared;

namespace UniTask.Api.Projects;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? OrganisationId { get; set; }
    public TaskProvider? Provider { get; set; }
    public Guid? TaskProviderAuthId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
