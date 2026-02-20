using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Commands.AssignMember;

public class AssignMemberToTaskCommandHandler : IRequestHandler<AssignMemberToTaskCommand, MemberAssignedToTaskEvent>
{
    private readonly ITasksAdapter _adapter;
    private readonly IPublisher _publisher;

    public AssignMemberToTaskCommandHandler(ITasksAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<MemberAssignedToTaskEvent> Handle(AssignMemberToTaskCommand request, CancellationToken cancellationToken)
    {
        var memberAssignedEvent = await _adapter.Handle(request);

        await _publisher.Publish(memberAssignedEvent, cancellationToken);

        return memberAssignedEvent;
    }
}
