using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Create;

public class CreateProjectCommand : IRequest<Guid>, IProviderEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? OrganisationId { get; set; }
    public TaskProvider? Provider { get; set; }
    public string? ExternalId { get; set; }
    public Guid? TaskProviderAuthId { get; set; }
    public bool TriggerSync { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
