using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Events;

public class OrganisationCreatedEvent : INotification, IProviderEvent
{
    public Guid OrganisationId { get; set; }
    public required string Name { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
