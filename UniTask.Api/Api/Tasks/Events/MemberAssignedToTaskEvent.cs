using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Events;

public class MemberAssignedToTaskEvent : INotification, IProviderEvent
{
    public Guid TaskId { get; set; }
    public required string AssignedTo { get; set; }
    public DateTime AssignedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
