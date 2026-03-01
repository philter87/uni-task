using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Events;

public class TaskProviderAuthCreatedEvent : INotification, IProviderEvent
{
    public Guid TaskProviderAuthId { get; set; }
    public Guid OrganisationId { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
