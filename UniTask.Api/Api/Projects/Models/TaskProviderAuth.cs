using UniTask.Api.Shared;

namespace UniTask.Api.Projects;

public class TaskProviderAuth
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public required string AuthTypeId { get; set; }
    public required string SecretValue { get; set; }

    // Navigation properties
    public Organisation Organisation { get; set; } = null!;
}
