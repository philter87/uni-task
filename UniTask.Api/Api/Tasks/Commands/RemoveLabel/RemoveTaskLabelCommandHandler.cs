using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.RemoveLabel;

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

        var label = task.Labels.FirstOrDefault(l => l.Id == request.LabelId);
        if (label != null)
        {
            task.Labels.Remove(label);
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        var labelRemovedEvent = new TaskLabelRemovedEvent
        {
            TaskId = request.TaskId,
            LabelId = request.LabelId,
            RemovedAt = DateTime.UtcNow
        };

        await _publisher.Publish(labelRemovedEvent, cancellationToken);

        return labelRemovedEvent;
    }
}
