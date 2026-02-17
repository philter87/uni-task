using MediatR;
using UniTask.Api.Shared.Adapters;

namespace UniTask.Api.Tasks.RemoveLabel;

public class RemoveTaskLabelCommandHandler : IRequestHandler<RemoveTaskLabelCommand, TaskItemDto>
{
    private readonly ITaskAdapter _adapter;
    private readonly IPublisher _publisher;

    public RemoveTaskLabelCommandHandler(ITaskAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskItemDto> Handle(RemoveTaskLabelCommand request, CancellationToken cancellationToken)
    {
        var task = await _adapter.RemoveLabelFromTaskAsync(request.TaskId, request.LabelId);
        
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        var labelRemovedEvent = new TaskLabelRemovedEvent
        {
            TaskId = task.Id,
            LabelId = request.LabelId,
            RemovedAt = DateTime.UtcNow
        };

        await _publisher.Publish(labelRemovedEvent, cancellationToken);

        return task;
    }
}
