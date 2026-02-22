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
        var dto = MemoryProviderMapper.MapToProjectDto(projectCreated);
        _projects[dto.Id] = dto;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ProjectDto>> GetProjects(GetProjectsQuery getProjects)
    {
        return Task.FromResult<IEnumerable<ProjectDto>>(_projects.Values);
    }

    public Task CreateTask(TaskCreatedEvent taskCreated)
    {
        var dto = MemoryProviderMapper.MapToTaskItemDto(taskCreated);
        _tasks[dto.Id] = dto;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TaskItemDto>> GetTasks(GetTasksQuery getTasks)
    {
        return Task.FromResult<IEnumerable<TaskItemDto>>(_tasks.Values);
    }
}
