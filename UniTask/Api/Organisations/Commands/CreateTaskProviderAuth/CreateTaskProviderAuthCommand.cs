using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Commands.CreateTaskProviderAuth;

public class CreateTaskProviderAuthCommand : IRequest, IProviderEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganisationId { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public required string AuthTypeId { get; set; }
    public required string SecretValue { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
