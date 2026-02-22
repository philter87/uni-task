using UniTask.Api.Shared;

namespace UniTask.Api.Projects;

public class TaskProviderSecret
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public TaskProvider Provider { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }

    // Navigation properties
    public Organisation Organisation { get; set; } = null!;
}
