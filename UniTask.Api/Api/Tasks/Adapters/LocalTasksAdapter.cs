using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Commands.AddLabel;
using UniTask.Api.Tasks.Commands.AssignMember;
using UniTask.Api.Tasks.Commands.ChangeStatus;
using UniTask.Api.Tasks.Commands.Create;
using UniTask.Api.Tasks.Commands.Delete;
using UniTask.Api.Tasks.Commands.RemoveLabel;
using UniTask.Api.Tasks.Commands.Update;
using UniTask.Api.Tasks.Queries.GetTask;
using UniTask.Api.Tasks.Queries.GetTasks;

namespace UniTask.Api.Tasks.Adapters;

public class LocalTasksAdapter : ITasksAdapter
{
    private readonly TaskDbContext _context;

    public LocalTasksAdapter(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<TaskCreatedEvent> Handle(CreateTaskCommand command)
    {
        TaskSource? source = null;
        if (!string.IsNullOrEmpty(command.Source))
        {
            Enum.TryParse<TaskSource>(command.Source, true, out var parsedSource);
            source = parsedSource;
        }

        var taskItem = new TaskItem
        {
            Title = command.Title,
            Description = command.Description,
            ProjectId = command.ProjectId,
            TaskTypeId = command.TaskTypeId,
            StatusId = command.StatusId,
            BoardId = command.BoardId,
            Priority = command.Priority,
            DueDate = command.DueDate,
            AssignedTo = command.AssignedTo,
            AssignedToUserId = command.AssignedToUserId,
            Source = source,
            ExternalId = command.ExternalId,
            DurationHours = command.DurationHours,
            DurationRemainingHours = command.DurationRemainingHours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(taskItem);
        await _context.SaveChangesAsync();

        return new TaskCreatedEvent
        {
            TaskId = taskItem.Id,
            Title = taskItem.Title,
            CreatedAt = taskItem.CreatedAt
        };
    }

    public async Task<TaskUpdatedEvent> Handle(UpdateTaskCommand command)
    {
        var existingTask = await _context.Tasks.FindAsync(command.TaskId);
        if (existingTask == null)
        {
            throw new InvalidOperationException($"Task with ID {command.TaskId} not found");
        }

        existingTask.Title = command.Title;
        existingTask.Description = command.Description;
        existingTask.StatusId = command.StatusId;
        existingTask.Priority = command.Priority;
        existingTask.DueDate = command.DueDate;
        existingTask.AssignedTo = command.AssignedTo;
        existingTask.ProjectId = command.ProjectId;
        existingTask.TaskTypeId = command.TaskTypeId;
        existingTask.BoardId = command.BoardId;
        existingTask.DurationHours = command.DurationHours;
        existingTask.DurationRemainingHours = command.DurationRemainingHours;
        existingTask.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TaskExists(command.TaskId))
            {
                throw new InvalidOperationException($"Task with ID {command.TaskId} not found");
            }
            throw;
        }

        return new TaskUpdatedEvent
        {
            TaskId = existingTask.Id,
            Title = existingTask.Title,
            UpdatedAt = existingTask.UpdatedAt
        };
    }

    public async Task<TaskDeletedEvent> Handle(DeleteTaskCommand command)
    {
        var task = await _context.Tasks.FindAsync(command.TaskId);
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {command.TaskId} not found");
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return new TaskDeletedEvent
        {
            TaskId = command.TaskId,
            DeletedAt = DateTime.UtcNow
        };
    }

    public async Task<TaskStatusChangedEvent> Handle(ChangeTaskStatusCommand command)
    {
        var task = await _context.Tasks.FindAsync(command.TaskId);
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {command.TaskId} not found");
        }

        task.StatusId = command.StatusId;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new TaskStatusChangedEvent
        {
            TaskId = command.TaskId,
            StatusId = command.StatusId,
            ChangedAt = DateTime.UtcNow
        };
    }

    public async Task<MemberAssignedToTaskEvent> Handle(AssignMemberToTaskCommand command)
    {
        var task = await _context.Tasks.FindAsync(command.TaskId);
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {command.TaskId} not found");
        }

        task.AssignedTo = command.AssignedTo;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new MemberAssignedToTaskEvent
        {
            TaskId = command.TaskId,
            AssignedTo = command.AssignedTo,
            AssignedAt = DateTime.UtcNow
        };
    }

    public async Task<TaskLabelAddedEvent> Handle(AddTaskLabelCommand command)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == command.TaskId);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {command.TaskId} not found");
        }

        var label = await _context.Labels.FindAsync(command.LabelId);
        if (label == null)
        {
            throw new InvalidOperationException($"Label with ID {command.LabelId} not found");
        }

        if (!task.Labels.Any(l => l.Id == command.LabelId))
        {
            task.Labels.Add(label);
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return new TaskLabelAddedEvent
        {
            TaskId = command.TaskId,
            LabelId = command.LabelId,
            AddedAt = DateTime.UtcNow
        };
    }

    public async Task<TaskLabelRemovedEvent> Handle(RemoveTaskLabelCommand command)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == command.TaskId);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {command.TaskId} not found");
        }

        var label = task.Labels.FirstOrDefault(l => l.Id == command.LabelId);
        if (label != null)
        {
            task.Labels.Remove(label);
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return new TaskLabelRemovedEvent
        {
            TaskId = command.TaskId,
            LabelId = command.LabelId,
            RemovedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<TaskItemDto>> Handle(GetTasksQuery query)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Board)
            .Include(t => t.Labels).ThenInclude(l => l.LabelType)
            .Include(t => t.Tags)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskItemDto?> Handle(GetTaskQuery query)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Board)
            .Include(t => t.Labels).ThenInclude(l => l.LabelType)
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == query.Id);

        return task == null ? null : MapToDto(task);
    }

    private async Task<bool> TaskExists(int id)
    {
        return await _context.Tasks.AnyAsync(e => e.Id == id);
    }

    private static TaskItemDto MapToDto(TaskItem task)
    {
        return new TaskItemDto
        {
            Id = task.Id,
            ExternalId = task.ExternalId,
            Title = task.Title,
            Description = task.Description,
            ProjectId = task.ProjectId,
            TaskTypeId = task.TaskTypeId,
            StatusId = task.StatusId,
            BoardId = task.BoardId,
            ParentId = task.ParentId,
            Priority = task.Priority,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate,
            AssignedTo = task.AssignedTo,
            AssignedToUserId = task.AssignedToUserId,
            Source = task.Source?.ToString(),
            DurationHours = task.DurationHours,
            DurationRemainingHours = task.DurationRemainingHours,
            Project = task.Project == null ? null : new ProjectDto
            {
                Id = task.Project.Id,
                ExternalId = task.Project.ExternalId,
                Name = task.Project.Name,
                Description = task.Project.Description,
                CreatedAt = task.Project.CreatedAt,
                UpdatedAt = task.Project.UpdatedAt
            },
            TaskType = task.TaskType == null ? null : new TaskTypeDto
            {
                Id = task.TaskType.Id,
                ExternalId = task.TaskType.ExternalId,
                Name = task.TaskType.Name,
                Description = task.TaskType.Description,
                ProjectId = task.TaskType.ProjectId
            },
            Status = task.Status == null ? null : new StatusDto
            {
                Id = task.Status.Id,
                ExternalId = task.Status.ExternalId,
                Name = task.Status.Name,
                Description = task.Status.Description,
                Order = task.Status.Order,
                TaskTypeId = task.Status.TaskTypeId
            },
            Board = task.Board == null ? null : new BoardDto
            {
                Id = task.Board.Id,
                ExternalId = task.Board.ExternalId,
                Name = task.Board.Name,
                Goal = task.Board.Goal,
                StartDate = task.Board.StartDate,
                EndDate = task.Board.EndDate,
                ProjectId = task.Board.ProjectId
            },
            Comments = task.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                TaskItemId = c.TaskItemId,
                Content = c.Content,
                UserId = c.UserId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            Labels = task.Labels.Select(l => new LabelDto
            {
                Id = l.Id,
                Name = l.Name,
                TypeId = l.TypeId,
                LabelType = l.LabelType == null ? null : new LabelTypeDto
                {
                    Id = l.LabelType.Id,
                    Name = l.LabelType.Name,
                    Color = l.LabelType.Color
                }
            }).ToList(),
            Tags = task.Tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
    }
}
