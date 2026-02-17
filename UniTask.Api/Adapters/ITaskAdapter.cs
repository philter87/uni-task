using UniTask.Api.DTOs;

namespace UniTask.Api.Adapters;

public interface ITaskAdapter
{
    Task<IEnumerable<TaskItemDto>> GetAllTasksAsync();
    Task<TaskItemDto?> GetTaskByIdAsync(int id);
    Task<TaskItemDto> CreateTaskAsync(TaskItemDto taskDto);
    Task<bool> UpdateTaskAsync(int id, TaskItemDto taskDto);
    Task<bool> DeleteTaskAsync(int id);
}
