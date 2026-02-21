using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Commands.Update;

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

        existingTask.Title = request.Title;
        existingTask.Description = request.Description;
        existingTask.StatusId = request.StatusId;
        existingTask.Priority = request.Priority;
        existingTask.DueDate = request.DueDate;
        existingTask.AssignedTo = request.AssignedTo;
        existingTask.ProjectId = request.ProjectId;
        existingTask.TaskTypeId = request.TaskTypeId;
        existingTask.BoardId = request.BoardId;
        existingTask.DurationHours = request.DurationHours;
        existingTask.DurationRemainingHours = request.DurationRemainingHours;
        existingTask.UpdatedAt = DateTime.UtcNow;

        try
        {
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

        var taskUpdatedEvent = new TaskUpdatedEvent
        {
            TaskId = existingTask.Id,
            Title = existingTask.Title,
            UpdatedAt = existingTask.UpdatedAt
        };

        await _publisher.Publish(taskUpdatedEvent, cancellationToken);

        return taskUpdatedEvent;
    }
}
