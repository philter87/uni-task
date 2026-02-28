using MediatR;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.AssignMember;

public class AssignMemberToTaskCommandHandler : IRequestHandler<AssignMemberToTaskCommand, MemberAssignedToTaskEvent>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public AssignMemberToTaskCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<MemberAssignedToTaskEvent> Handle(AssignMemberToTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks.FindAsync(request.TaskId);
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        task.AssignedTo = request.AssignedTo;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var memberAssignedEvent = new MemberAssignedToTaskEvent
        {
            TaskId = request.TaskId,
            AssignedTo = request.AssignedTo,
            AssignedAt = DateTime.UtcNow
        };

        await _publisher.Publish(memberAssignedEvent, cancellationToken);

        return memberAssignedEvent;
    }
}
