using MediatR;
using UniTask.Api.Shared.Adapters;

namespace UniTask.Api.Tasks.ChangeStatus;

public class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand, TaskItemDto>
{
    private readonly ITaskAdapter _adapter;
    private readonly IPublisher _publisher;

    public ChangeTaskStatusCommandHandler(ITaskAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskItemDto> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _adapter.ChangeTaskStatusAsync(request.TaskId, request.StatusId);
        
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        var taskStatusChangedEvent = new TaskStatusChangedEvent
        {
            TaskId = task.Id,
            StatusId = request.StatusId,
            ChangedAt = DateTime.UtcNow
        };

        await _publisher.Publish(taskStatusChangedEvent, cancellationToken);

        return task;
    }
}
