using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Commands.Create;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskCreatedEvent>
{
    private readonly ITasksAdapter _adapter;
    private readonly IPublisher _publisher;

    public CreateTaskCommandHandler(ITasksAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskCreatedEvent> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskCreatedEvent = await _adapter.Handle(request);

        await _publisher.Publish(taskCreatedEvent, cancellationToken);

        return taskCreatedEvent;
    }
}
