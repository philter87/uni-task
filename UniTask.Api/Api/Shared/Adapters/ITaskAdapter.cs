using UniTask.Api.Projects;
using UniTask.Api.Tasks;

namespace UniTask.Api.Shared.Adapters;

public interface ITaskAdapter
{
    Task<IEnumerable<TaskItemDto>> GetAllTasksAsync();
    Task<TaskItemDto?> GetTaskByIdAsync(int id);
    Task<TaskItemDto> CreateTaskAsync(TaskItemDto taskDto);
    Task<bool> UpdateTaskAsync(int id, TaskItemDto taskDto);
    Task<bool> DeleteTaskAsync(int id);
    
    // Project operations
    Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto);
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<ProjectDto?> GetProjectByIdAsync(int id);
}
