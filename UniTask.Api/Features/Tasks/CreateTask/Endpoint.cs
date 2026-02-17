using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Infrastructure.Adapters;
using UniTask.Api.DTOs;

namespace UniTask.Api.Features.Tasks.CreateTask;

[ApiController]
[Route("api/tasks")]
public class Endpoint : ControllerBase
{
    private readonly ITaskAdapter _adapter;

    public Endpoint(ITaskAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpPost]
    public async Task<ActionResult<Response>> CreateTask([FromBody] Request request)
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
        
        var createdTask = await _adapter.CreateTaskAsync(taskDto);
        
        var response = new Response
        {
            Id = createdTask.Id,
            Title = createdTask.Title,
            Description = createdTask.Description,
            ProjectId = createdTask.ProjectId,
            TaskTypeId = createdTask.TaskTypeId,
            StatusId = createdTask.StatusId,
            SprintId = createdTask.SprintId,
            Priority = createdTask.Priority,
            CreatedAt = createdTask.CreatedAt,
            UpdatedAt = createdTask.UpdatedAt,
            DueDate = createdTask.DueDate,
            AssignedTo = createdTask.AssignedTo,
            Source = createdTask.Source,
            ExternalId = createdTask.ExternalId,
            DurationMin = createdTask.DurationMin,
            RemainingMin = createdTask.RemainingMin
        };
        
        return CreatedAtRoute("GetTask", new { id = response.Id }, response);
    }
}
