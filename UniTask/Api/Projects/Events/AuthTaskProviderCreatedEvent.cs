using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Events;

public class AuthTaskProviderCreatedEvent : INotification, IProviderEvent
{
    public Guid AuthTaskProviderId { get; set; }
    public Guid OrganisationId { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
