using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Delete;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, TaskDeletedEvent>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public DeleteTaskCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<TaskDeletedEvent> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks.FindAsync(request.TaskId);
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        task.Delete(request);
        _context.Tasks.Remove(task);

        await _publisher.PublishAll(task.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return task.DomainEvents.OfType<TaskDeletedEvent>().First();
    }
}
