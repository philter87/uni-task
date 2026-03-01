using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.AssignMember;

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

        task.AssignMember(request);

        await _publisher.PublishAll(task.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return task.DomainEvents.OfType<MemberAssignedToTaskEvent>().First();
    }
}
