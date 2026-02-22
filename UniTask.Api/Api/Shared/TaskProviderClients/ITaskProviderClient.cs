using UniTask.Api.Projects;
using UniTask.Api.Projects.Events;
using UniTask.Api.Projects.Queries.GetProjects;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Events;
using UniTask.Api.Tasks.Queries.GetTasks;

namespace UniTask.Api.Shared.TaskProviderClients;

public interface ITaskProviderClient
{
    Task CreateProject(ProjectCreatedEvent projectCreated);
    Task<IEnumerable<ProjectDto>> GetProjects(GetProjectsQuery getProjects);

    Task CreateTask(TaskCreatedEvent taskCreated);
    Task<IEnumerable<TaskItemDto>> GetTasks(GetTasksQuery getTasks);
}
