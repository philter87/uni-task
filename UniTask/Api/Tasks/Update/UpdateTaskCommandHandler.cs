using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Update;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskUpdatedEvent>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public UpdateTaskCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<TaskUpdatedEvent> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var existingTask = await _context.Tasks.FindAsync(request.TaskId);
        if (existingTask == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        existingTask.Update(request);

        try
        {
            await _publisher.PublishAll(existingTask.DomainEvents, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Tasks.AnyAsync(e => e.Id == request.TaskId, cancellationToken))
            {
                throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
            }
            throw;
        }

        return existingTask.DomainEvents.OfType<TaskUpdatedEvent>().First();
    }
}
