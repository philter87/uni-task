using UniTask.Api.Projects;
using UniTask.Api.Projects.Events;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Shared.TaskProviderClients;

public static class MemoryProviderMapper
{
    public static ProjectDto MapToProjectDto(ProjectCreatedEvent projectCreated)
    {
        return new ProjectDto
        {
            Id = projectCreated.ProjectId,
            Name = projectCreated.Name,
            Description = projectCreated.Description,
            CreatedAt = projectCreated.CreatedAt,
            UpdatedAt = projectCreated.CreatedAt
        };
    }

    public static TaskItemDto MapToTaskItemDto(TaskCreatedEvent taskCreated)
    {
        return new TaskItemDto
        {
            Id = taskCreated.TaskId,
            Title = taskCreated.Title,
            CreatedAt = taskCreated.CreatedAt,
            UpdatedAt = taskCreated.CreatedAt
        };
    }
}
