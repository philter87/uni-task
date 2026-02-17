using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Infrastructure.Adapters;

namespace UniTask.Api.Features.Tasks.GetTasks;

[ApiController]
[Route("api/tasks")]
public class Endpoint : ControllerBase
{
    private readonly ITaskAdapter _adapter;

    public Endpoint(ITaskAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Response>>> GetTasks()
    {
        var tasks = await _adapter.GetAllTasksAsync();
        var response = tasks.Select(t => new Response
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
