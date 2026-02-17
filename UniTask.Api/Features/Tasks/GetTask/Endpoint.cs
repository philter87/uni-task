using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Infrastructure.Adapters;

namespace UniTask.Api.Features.Tasks.GetTask;

[ApiController]
[Route("api/tasks")]
public class Endpoint : ControllerBase
{
    private readonly ITaskAdapter _adapter;

    public Endpoint(ITaskAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpGet("{id}", Name = "GetTask")]
    public async Task<ActionResult<Response>> GetTask(int id)
    {
        var task = await _adapter.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        
        var response = new Response
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ProjectId = task.ProjectId,
            TaskTypeId = task.TaskTypeId,
            StatusId = task.StatusId,
            SprintId = task.SprintId,
            Priority = task.Priority,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate,
            AssignedTo = task.AssignedTo,
            Source = task.Source,
            ExternalId = task.ExternalId,
            DurationMin = task.DurationMin,
            RemainingMin = task.RemainingMin
        };
        
        return Ok(response);
    }
}
