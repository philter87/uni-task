using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Models;

public class TaskProviderAuth
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public required string AuthTypeId { get; set; }
    public required string SecretValue { get; set; }

    // Navigation properties
    public Organisation Organisation { get; set; } = null!;
}
