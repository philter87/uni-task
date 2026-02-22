using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.AddLabel;

public class AddTaskLabelCommandHandler : IRequestHandler<AddTaskLabelCommand, TaskLabelAddedEvent>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public AddTaskLabelCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<TaskLabelAddedEvent> Handle(AddTaskLabelCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        var label = await _context.Labels.FindAsync(request.LabelId);
        if (label == null)
        {
            throw new InvalidOperationException($"Label with ID {request.LabelId} not found");
        }

        if (!task.Labels.Any(l => l.Id == request.LabelId))
        {
            task.Labels.Add(label);
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        var labelAddedEvent = new TaskLabelAddedEvent
        {
            TaskId = request.TaskId,
            LabelId = request.LabelId,
            AddedAt = DateTime.UtcNow
        };

        await _publisher.Publish(labelAddedEvent, cancellationToken);

        return labelAddedEvent;
    }
}
