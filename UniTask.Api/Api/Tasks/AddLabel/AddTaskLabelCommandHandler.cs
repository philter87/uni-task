using MediatR;
using UniTask.Api.Shared.Adapters;

namespace UniTask.Api.Tasks.AddLabel;

public class AddTaskLabelCommandHandler : IRequestHandler<AddTaskLabelCommand, TaskItemDto>
{
    private readonly ITaskAdapter _adapter;
    private readonly IPublisher _publisher;

    public AddTaskLabelCommandHandler(ITaskAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskItemDto> Handle(AddTaskLabelCommand request, CancellationToken cancellationToken)
    {
        var task = await _adapter.AddLabelToTaskAsync(request.TaskId, request.LabelId);
        
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found or label with ID {request.LabelId} not found");
        }

        var labelAddedEvent = new TaskLabelAddedEvent
        {
            TaskId = task.Id,
            LabelId = request.LabelId,
            AddedAt = DateTime.UtcNow
        };

        await _publisher.Publish(labelAddedEvent, cancellationToken);

        return task;
    }
}
