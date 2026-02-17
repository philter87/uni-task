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
    
    // Task operations
    Task<TaskItemDto?> ChangeTaskStatusAsync(int taskId, int statusId);
    Task<TaskItemDto?> AssignMemberToTaskAsync(int taskId, string assignedTo);
    Task<TaskItemDto?> AddLabelToTaskAsync(int taskId, int labelId);
    Task<TaskItemDto?> RemoveLabelFromTaskAsync(int taskId, int labelId);
    
    // Project operations
    Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto);
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<ProjectDto?> GetProjectByIdAsync(int id);
}
