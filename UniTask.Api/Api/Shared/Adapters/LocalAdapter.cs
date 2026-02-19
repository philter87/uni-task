using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Shared;
using UniTask.Api.Tasks;

namespace UniTask.Api.Shared.Adapters;

public class LocalAdapter : ITaskAdapter
{
    private readonly TaskDbContext _context;

    public LocalAdapter(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItemDto>> GetAllTasksAsync()
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

    public async Task<TaskItemDto?> GetTaskByIdAsync(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Board)
            .Include(t => t.Labels).ThenInclude(l => l.LabelType)
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

        return task == null ? null : MapToDto(task);
    }

    public async Task<TaskItemDto> CreateTaskAsync(TaskItemDto taskDto)
    {
        var taskItem = MapToEntity(taskDto);
        taskItem.CreatedAt = DateTime.UtcNow;
        taskItem.UpdatedAt = DateTime.UtcNow;

        _context.Tasks.Add(taskItem);
        await _context.SaveChangesAsync();

        return MapToDto(taskItem);
    }

    public async Task<bool> UpdateTaskAsync(int id, TaskItemDto taskDto)
    {
        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null)
        {
            return false;
        }

        existingTask.Title = taskDto.Title;
        existingTask.Description = taskDto.Description;
        existingTask.StatusId = taskDto.StatusId;
        existingTask.Priority = taskDto.Priority;
        existingTask.DueDate = taskDto.DueDate;
        existingTask.AssignedTo = taskDto.AssignedTo;
        existingTask.AssignedToUserId = taskDto.AssignedToUserId;
        existingTask.ProjectId = taskDto.ProjectId;
        existingTask.TaskTypeId = taskDto.TaskTypeId;
        existingTask.BoardId = taskDto.BoardId;
        existingTask.ParentId = taskDto.ParentId;
        existingTask.DurationHours = taskDto.DurationHours;
        existingTask.DurationRemainingHours = taskDto.DurationRemainingHours;
        existingTask.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TaskExists(id))
            {
                return false;
            }
            throw;
        }
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<TaskItemDto?> ChangeTaskStatusAsync(int taskId, int statusId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return null;
        }

        task.StatusId = statusId;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(taskId);
    }

    public async Task<TaskItemDto?> AssignMemberToTaskAsync(int taskId, string assignedTo)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return null;
        }

        task.AssignedTo = assignedTo;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(taskId);
    }

    public async Task<TaskItemDto?> AddLabelToTaskAsync(int taskId, int labelId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels).ThenInclude(l => l.LabelType)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        
        if (task == null)
        {
            return null;
        }

        var label = await _context.Labels.FindAsync(labelId);
        if (label == null)
        {
            return null;
        }

        // Check if the label is already added
        if (!task.Labels.Any(l => l.Id == labelId))
        {
            task.Labels.Add(label);
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetTaskByIdAsync(taskId);
        }

        // Label already exists, return task without making changes
        return MapToDto(task);
    }

    public async Task<TaskItemDto?> RemoveLabelFromTaskAsync(int taskId, int labelId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels).ThenInclude(l => l.LabelType)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        
        if (task == null)
        {
            return null;
        }

        var label = task.Labels.FirstOrDefault(l => l.Id == labelId);
        if (label != null)
        {
            task.Labels.Remove(label);
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return await GetTaskByIdAsync(taskId);
    }

    public async Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto)
    {
        var project = new Project
        {
            Name = projectDto.Name,
            Description = projectDto.Description,
            ExternalId = projectDto.ExternalId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return new ProjectDto
        {
            Id = project.Id,
            ExternalId = project.ExternalId,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        var projects = await _context.Projects.ToListAsync();
        
        return projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            ExternalId = p.ExternalId,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        
        if (project == null)
        {
            return null;
        }

        return new ProjectDto
        {
            Id = project.Id,
            ExternalId = project.ExternalId,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }

    private async Task<bool> TaskExists(int id)
    {
        return await _context.Tasks.AnyAsync(e => e.Id == id);
    }

    /// <summary>
    /// Maps a TaskItem entity to a TaskItemDto including all navigation properties.
    /// </summary>
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

    /// <summary>
    /// Maps a TaskItemDto to a TaskItem entity for database operations.
    /// </summary>
    private static TaskItem MapToEntity(TaskItemDto dto)
    {
        TaskSource? source = null;
        if (!string.IsNullOrEmpty(dto.Source))
        {
            Enum.TryParse<TaskSource>(dto.Source, true, out var parsedSource);
            source = parsedSource;
        }

        return new TaskItem
        {
            Id = dto.Id,
            ExternalId = dto.ExternalId,
            Title = dto.Title,
            Description = dto.Description,
            ProjectId = dto.ProjectId,
            TaskTypeId = dto.TaskTypeId,
            StatusId = dto.StatusId,
            BoardId = dto.BoardId,
            ParentId = dto.ParentId,
            Priority = dto.Priority,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            DueDate = dto.DueDate,
            AssignedTo = dto.AssignedTo,
            AssignedToUserId = dto.AssignedToUserId,
            Source = source,
            DurationHours = dto.DurationHours,
            DurationRemainingHours = dto.DurationRemainingHours
        };
    }
}
