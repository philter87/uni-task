using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Commands.AddLabel;

public class AddTaskLabelCommandHandler : IRequestHandler<AddTaskLabelCommand, TaskLabelAddedEvent>
{
    private readonly ITasksAdapter _adapter;
    private readonly IPublisher _publisher;

    public AddTaskLabelCommandHandler(ITasksAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskLabelAddedEvent> Handle(AddTaskLabelCommand request, CancellationToken cancellationToken)
    {
        var labelAddedEvent = await _adapter.Handle(request);

        await _publisher.Publish(labelAddedEvent, cancellationToken);

        return labelAddedEvent;
    }
}
