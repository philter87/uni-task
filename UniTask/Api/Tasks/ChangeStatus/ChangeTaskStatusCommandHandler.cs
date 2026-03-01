using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.ChangeStatus;

public class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand, TaskStatusChangedEvent>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public ChangeTaskStatusCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<TaskStatusChangedEvent> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks.FindAsync(request.TaskId);
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        task.ChangeStatus(request);

        await _publisher.PublishAll(task.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return task.DomainEvents.OfType<TaskStatusChangedEvent>().First();
    }
}
