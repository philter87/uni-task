using MediatR;
using UniTask.Api.Shared.Adapters;

namespace UniTask.Api.Tasks.Update;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskItemDto>
{
    private readonly ITaskAdapter _adapter;
    private readonly IPublisher _publisher;

    public UpdateTaskCommandHandler(ITaskAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<TaskItemDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskDto = new TaskItemDto
        {
            Id = request.TaskId,
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            TaskTypeId = request.TaskTypeId,
            StatusId = request.StatusId,
            BoardId = request.BoardId,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedTo = request.AssignedTo,
            DurationHours = request.DurationHours,
            DurationRemainingHours = request.DurationRemainingHours
        };

        var success = await _adapter.UpdateTaskAsync(request.TaskId, taskDto);
        
        if (!success)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");
        }

        var updatedTask = await _adapter.GetTaskByIdAsync(request.TaskId);
        
        if (updatedTask == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found after update");
        }

        var taskUpdatedEvent = new TaskUpdatedEvent
        {
            TaskId = updatedTask.Id,
            Title = updatedTask.Title,
            UpdatedAt = DateTime.UtcNow
        };

        await _publisher.Publish(taskUpdatedEvent, cancellationToken);

        return updatedTask;
    }
}
