using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Commands.Create;

public class CreateOrganisationCommand : IRequest, IProviderEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? ExternalId { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
