using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.RemoveLabel;

public class RemoveTaskLabelCommandHandler : IRequestHandler<RemoveTaskLabelCommand, TaskLabelRemovedEvent>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public RemoveTaskLabelCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<TaskLabelRemovedEvent> Handle(RemoveTaskLabelCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        task.RemoveLabel(request.LabelId, request);

        await _publisher.PublishAll(task.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return task.DomainEvents.OfType<TaskLabelRemovedEvent>().First();
    }
}
