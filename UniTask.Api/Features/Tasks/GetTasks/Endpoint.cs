using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Infrastructure.Adapters;

namespace UniTask.Api.Features.Tasks.GetTasks;

[ApiController]
[Route("api/tasks")]
public class GetTasksController : ControllerBase
{
    private readonly ITaskAdapter _adapter;

    public GetTasksController(ITaskAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetTasksResponse>>> GetTasks()
    {
        var tasks = await _adapter.GetAllTasksAsync();
        var response = tasks.Select(t => new GetTasksResponse
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            ProjectId = t.ProjectId,
            TaskTypeId = t.TaskTypeId,
            StatusId = t.StatusId,
            SprintId = t.SprintId,
            Priority = t.Priority,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            DueDate = t.DueDate,
            AssignedTo = t.AssignedTo,
            Source = t.Source,
            ExternalId = t.ExternalId,
            DurationMin = t.DurationMin,
            RemainingMin = t.RemainingMin
        });
        return Ok(response);
    }
}
