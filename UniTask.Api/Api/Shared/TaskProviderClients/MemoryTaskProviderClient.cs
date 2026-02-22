using System.Collections.Concurrent;
using UniTask.Api.Projects;
using UniTask.Api.Projects.Events;
using UniTask.Api.Projects.Queries.GetProjects;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Events;
using UniTask.Api.Tasks.Queries.GetTasks;

namespace UniTask.Api.Shared.TaskProviderClients;

public class MemoryTaskProviderClient : ITaskProviderClient
{
    private readonly ConcurrentDictionary<int, ProjectDto> _projects = new();
    private readonly ConcurrentDictionary<int, TaskItemDto> _tasks = new();

    public Task CreateProject(ProjectCreatedEvent projectCreated)
    {
        var dto = new ProjectDto
        {
            Id = projectCreated.ProjectId,
            Name = projectCreated.Name,
            Description = projectCreated.Description,
            CreatedAt = projectCreated.CreatedAt,
            UpdatedAt = projectCreated.CreatedAt
        };
        _projects[dto.Id] = dto;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ProjectDto>> GetProjects(GetProjectsQuery getProjects)
    {
        return Task.FromResult<IEnumerable<ProjectDto>>(_projects.Values);
    }

    public Task CreateTask(TaskCreatedEvent taskCreated)
    {
        var dto = new TaskItemDto
        {
            Id = taskCreated.TaskId,
            Title = taskCreated.Title,
            CreatedAt = taskCreated.CreatedAt,
            UpdatedAt = taskCreated.CreatedAt
        };
        _tasks[dto.Id] = dto;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TaskItemDto>> GetTasks(GetTasksQuery getTasks)
    {
        return Task.FromResult<IEnumerable<TaskItemDto>>(_tasks.Values);
    }
}
