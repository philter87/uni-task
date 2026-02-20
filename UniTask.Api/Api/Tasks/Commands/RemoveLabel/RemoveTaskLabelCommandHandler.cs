using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Commands.RemoveLabel;

public class RemoveTaskLabelCommandHandler : IRequestHandler<RemoveTaskLabelCommand, TaskLabelRemovedEvent>
{
    private readonly ITasksAdapter _adapter;
    private readonly IPublisher _publisher;

    public RemoveTaskLabelCommandHandler(ITasksAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskLabelRemovedEvent> Handle(RemoveTaskLabelCommand request, CancellationToken cancellationToken)
    {
        var labelRemovedEvent = await _adapter.Handle(request);

        await _publisher.Publish(labelRemovedEvent, cancellationToken);

        return labelRemovedEvent;
    }
}
