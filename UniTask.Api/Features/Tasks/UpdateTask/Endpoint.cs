using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Infrastructure.Adapters;
using UniTask.Api.DTOs;

namespace UniTask.Api.Features.Tasks.UpdateTask;

[ApiController]
[Route("api/tasks")]
public class UpdateTaskController : ControllerBase
{
    private readonly ITaskAdapter _adapter;

    public UpdateTaskController(ITaskAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskCommand request)
    {
        var taskDto = new TaskItemDto
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            TaskTypeId = request.TaskTypeId,
            StatusId = request.StatusId,
            SprintId = request.SprintId,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedTo = request.AssignedTo,
            Source = request.Source,
            ExternalId = request.ExternalId,
            DurationMin = request.DurationMin,
            RemainingMin = request.RemainingMin
        };
        
        var updated = await _adapter.UpdateTaskAsync(id, taskDto);
        if (!updated)
        {
            return NotFound();
        }
        return NoContent();
    }
}
