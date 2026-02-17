using MediatR;

namespace UniTask.Api.Tasks.AssignMember;

public class MemberAssignedToTaskEventHandler : INotificationHandler<MemberAssignedToTaskEvent>
{
    public Task Handle(MemberAssignedToTaskEvent notification, CancellationToken cancellationToken)
    {
        // Log or handle the event as needed
        // For now, just return completed task
        return Task.CompletedTask;
    }
}
