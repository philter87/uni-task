using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UniTask.Api.Shared.Adapters;
using UniTask.Api.Tasks.ChangeStatus;
using UniTask.Api.Tasks.AssignMember;
using UniTask.Api.Tasks.Update;
using UniTask.Api.Tasks.AddLabel;
using UniTask.Api.Tasks.RemoveLabel;

namespace UniTask.Api.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskAdapter _adapter;
    private readonly IMediator _mediator;

    public TasksController(ITaskAdapter adapter, IMediator mediator)
    {
        _adapter = adapter;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
    {
        var tasks = await _adapter.GetAllTasksAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDto>> GetTask(int id)
    {
        var task = await _adapter.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> CreateTask([FromBody] TaskItemDto taskDto)
    {
        var createdTask = await _adapter.CreateTaskAsync(taskDto);
        return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItemDto taskDto)
    {
        var updated = await _adapter.UpdateTaskAsync(id, taskDto);
        if (!updated)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var deleted = await _adapter.DeleteTaskAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TaskItemDto>> ChangeTaskStatus(int id, [FromBody] ChangeTaskStatusRequest request)
    {
        try
        {
            var command = new ChangeTaskStatusCommand
            {
                TaskId = id,
                StatusId = request.StatusId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id}/assign")]
    public async Task<ActionResult<TaskItemDto>> AssignMemberToTask(int id, [FromBody] AssignMemberRequest request)
    {
        try
        {
            var command = new AssignMemberToTaskCommand
            {
                TaskId = id,
                AssignedTo = request.AssignedTo
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}/update")]
    public async Task<ActionResult<TaskItemDto>> UpdateTaskWithCommand(int id, [FromBody] UpdateTaskRequest request)
    {
        try
        {
            var command = new UpdateTaskCommand
            {
                TaskId = id,
                Title = request.Title,
                Description = request.Description,
                ProjectId = request.ProjectId,
                TaskTypeId = request.TaskTypeId,
                StatusId = request.StatusId,
                BoardId = request.BoardId,
                Priority = request.Priority,
                DueDate = request.DueDate,
                AssignedTo = request.AssignedTo,
                DurationHours = request.DurationHours,
                DurationRemainingHours = request.DurationRemainingHours
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/labels/{labelId}")]
    public async Task<ActionResult<TaskItemDto>> AddTaskLabel(int id, int labelId)
    {
        try
        {
            var command = new AddTaskLabelCommand
            {
                TaskId = id,
                LabelId = labelId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}/labels/{labelId}")]
    public async Task<ActionResult<TaskItemDto>> RemoveTaskLabel(int id, int labelId)
    {
        try
        {
            var command = new RemoveTaskLabelCommand
            {
                TaskId = id,
                LabelId = labelId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public class ChangeTaskStatusRequest
{
    public int StatusId { get; set; }
}

public class AssignMemberRequest
{
    public required string AssignedTo { get; set; }
}

public class UpdateTaskRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskTypeId { get; set; }
    public int? StatusId { get; set; }
    public int? BoardId { get; set; }
    [Range(0, 10)]
    public double Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public double? DurationHours { get; set; }
    public double? DurationRemainingHours { get; set; }
}
