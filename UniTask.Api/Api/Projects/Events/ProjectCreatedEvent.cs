using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Events;

public class ProjectCreatedEvent : INotification, IProviderEvent
{
    public int ProjectId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
