using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Create;

public class CreateOrganisationCommand : IRequest<Guid>, IProviderEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? ExternalId { get; set; }
    public bool IsPersonal { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
