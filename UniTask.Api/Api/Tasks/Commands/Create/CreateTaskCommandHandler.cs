using MediatR;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.Create;

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
        var taskItem = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            TaskTypeId = request.TaskTypeId,
            StatusId = request.StatusId,
            BoardId = request.BoardId,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedTo = request.AssignedTo,
            AssignedToUserId = request.AssignedToUserId,
            Provider = request.Provider,
            ExternalId = request.ExternalId,
            DurationHours = request.DurationHours,
            DurationRemainingHours = request.DurationRemainingHours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(taskItem);
        await _context.SaveChangesAsync(cancellationToken);

        var taskCreatedEvent = new TaskCreatedEvent
        {
            TaskId = taskItem.Id,
            Title = taskItem.Title,
            CreatedAt = taskItem.CreatedAt
        };

        await _publisher.Publish(taskCreatedEvent, cancellationToken);

        return taskCreatedEvent;
    }
}
