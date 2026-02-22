using MediatR;

namespace UniTask.Api.Tasks.Events;

public class MemberAssignedToTaskEvent : INotification
{
    public int TaskId { get; set; }
    public required string AssignedTo { get; set; }
    public DateTime AssignedAt { get; set; }
}
