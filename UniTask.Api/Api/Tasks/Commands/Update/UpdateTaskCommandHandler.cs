using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Commands.Update;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskUpdatedEvent>
{
    private readonly ITasksAdapter _adapter;
    private readonly IPublisher _publisher;

    public UpdateTaskCommandHandler(ITasksAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskUpdatedEvent> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskUpdatedEvent = await _adapter.Handle(request);

        await _publisher.Publish(taskUpdatedEvent, cancellationToken);

        return taskUpdatedEvent;
    }
}
