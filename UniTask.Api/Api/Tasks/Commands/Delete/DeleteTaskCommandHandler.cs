using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Commands.Delete;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, TaskDeletedEvent>
{
    private readonly ITasksAdapter _adapter;
    private readonly IPublisher _publisher;

    public DeleteTaskCommandHandler(ITasksAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskDeletedEvent> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var taskDeletedEvent = await _adapter.Handle(request);

        await _publisher.Publish(taskDeletedEvent, cancellationToken);

        return taskDeletedEvent;
    }
}
