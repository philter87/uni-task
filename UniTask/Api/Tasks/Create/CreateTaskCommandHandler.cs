using MediatR;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Models;

namespace UniTask.Api.Tasks.Create;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskCreatedEvent>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public CreateTaskCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<TaskCreatedEvent> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskItem = TaskItem.Create(request);
        _context.Tasks.Add(taskItem);

        await _publisher.PublishAll(taskItem.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return taskItem.DomainEvents.OfType<TaskCreatedEvent>().First();
    }
}
