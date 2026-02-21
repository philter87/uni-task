using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Commands.ChangeStatus;

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

        task.StatusId = request.StatusId;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var taskStatusChangedEvent = new TaskStatusChangedEvent
        {
            TaskId = request.TaskId,
            StatusId = request.StatusId,
            ChangedAt = DateTime.UtcNow
        };

        await _publisher.Publish(taskStatusChangedEvent, cancellationToken);

        return taskStatusChangedEvent;
    }
}
