using MediatR;

namespace UniTask.Api.Tasks.Commands.AssignMember;

public class MemberAssignedToTaskEventHandler : INotificationHandler<MemberAssignedToTaskEvent>
{
    public Task Handle(MemberAssignedToTaskEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
