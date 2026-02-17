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
            .Include(t => t.Sprint)
            .Include(t => t.Labels)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskItemDto?> GetTaskByIdAsync(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Sprint)
            .Include(t => t.Labels)
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
        existingTask.Priority = ParsePriority(taskDto.Priority);
        existingTask.DueDate = taskDto.DueDate;
        existingTask.AssignedTo = taskDto.AssignedTo;
        existingTask.ProjectId = taskDto.ProjectId;
        existingTask.TaskTypeId = taskDto.TaskTypeId;
        existingTask.SprintId = taskDto.SprintId;
        existingTask.DurationMin = taskDto.DurationMin;
        existingTask.RemainingMin = taskDto.RemainingMin;
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
            .Include(t => t.Labels)
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
        }

        return await GetTaskByIdAsync(taskId);
    }

    public async Task<TaskItemDto?> RemoveLabelFromTaskAsync(int taskId, int labelId)
    {
        var task = await _context.Tasks
            .Include(t => t.Labels)
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return new ProjectDto
        {
            Id = project.Id,
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
            Title = task.Title,
            Description = task.Description,
            ProjectId = task.ProjectId,
            TaskTypeId = task.TaskTypeId,
            StatusId = task.StatusId,
            SprintId = task.SprintId,
            Priority = task.Priority.ToString(),
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate,
            AssignedTo = task.AssignedTo,
            Source = task.Source,
            ExternalId = task.ExternalId,
            DurationMin = task.DurationMin,
            RemainingMin = task.RemainingMin,
            Project = task.Project == null ? null : new ProjectDto
            {
                Id = task.Project.Id,
                Name = task.Project.Name,
                Description = task.Project.Description,
                CreatedAt = task.Project.CreatedAt,
                UpdatedAt = task.Project.UpdatedAt
            },
            TaskType = task.TaskType == null ? null : new TaskTypeDto
            {
                Id = task.TaskType.Id,
                Name = task.TaskType.Name,
                Description = task.TaskType.Description,
                ProjectId = task.TaskType.ProjectId
            },
            Status = task.Status == null ? null : new StatusDto
            {
                Id = task.Status.Id,
                Name = task.Status.Name,
                Description = task.Status.Description,
                Order = task.Status.Order,
                ProjectId = task.Status.ProjectId
            },
            Sprint = task.Sprint == null ? null : new SprintDto
            {
                Id = task.Sprint.Id,
                Name = task.Sprint.Name,
                Goal = task.Sprint.Goal,
                StartDate = task.Sprint.StartDate,
                EndDate = task.Sprint.EndDate,
                ProjectId = task.Sprint.ProjectId
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
                Color = l.Color
            }).ToList()
        };
    }

    /// <summary>
    /// Maps a TaskItemDto to a TaskItem entity for database operations.
    /// </summary>
    private static TaskItem MapToEntity(TaskItemDto dto)
    {
        return new TaskItem
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            ProjectId = dto.ProjectId,
            TaskTypeId = dto.TaskTypeId,
            StatusId = dto.StatusId,
            SprintId = dto.SprintId,
            Priority = ParsePriority(dto.Priority),
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            DueDate = dto.DueDate,
            AssignedTo = dto.AssignedTo,
            Source = dto.Source,
            ExternalId = dto.ExternalId,
            DurationMin = dto.DurationMin,
            RemainingMin = dto.RemainingMin
        };
    }

    /// <summary>
    /// Parses a priority string to TaskPriority enum value.
    /// Performs case-insensitive parsing and defaults to Medium for invalid values.
    /// </summary>
    private static TaskPriority ParsePriority(string priority)
    {
        return Enum.TryParse<TaskPriority>(priority, true, out var result) 
            ? result 
            : TaskPriority.Medium;
    }
}
