using MediatR;
using UniTask.Api.Shared.Adapters;

namespace UniTask.Api.Tasks.AssignMember;

public class AssignMemberToTaskCommandHandler : IRequestHandler<AssignMemberToTaskCommand, TaskItemDto>
{
    private readonly ITaskAdapter _adapter;
    private readonly IPublisher _publisher;

    public AssignMemberToTaskCommandHandler(ITaskAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskItemDto> Handle(AssignMemberToTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _adapter.AssignMemberToTaskAsync(request.TaskId, request.AssignedTo);
        
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        var memberAssignedEvent = new MemberAssignedToTaskEvent
        {
            TaskId = task.Id,
            AssignedTo = request.AssignedTo,
            AssignedAt = DateTime.UtcNow
        };

        await _publisher.Publish(memberAssignedEvent, cancellationToken);

        return task;
    }
}
