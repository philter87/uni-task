using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Commands.ChangeStatus;

public class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand, TaskStatusChangedEvent>
{
    private readonly ITasksAdapter _adapter;
    private readonly IPublisher _publisher;

    public ChangeTaskStatusCommandHandler(ITasksAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskStatusChangedEvent> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var taskStatusChangedEvent = await _adapter.Handle(request);

        await _publisher.Publish(taskStatusChangedEvent, cancellationToken);

        return taskStatusChangedEvent;
    }
}
