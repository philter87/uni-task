using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Adapters;
using UniTask.Api.DTOs;
using UniTask.Api.Models;
using UniTask.Api.Services;

namespace UniTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskAdapter _adapter;
    private readonly IChangeEventService _changeEventService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskAdapter adapter, IChangeEventService changeEventService, ILogger<TasksController> logger)
    {
        _adapter = adapter;
        _changeEventService = changeEventService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tasks
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
    {
        var tasks = await _adapter.GetAllTasksAsync();
        return Ok(tasks);
    }

    /// <summary>
    /// Get a specific task by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDto>> GetTask(int id)
    {
        var task = await _adapter.GetTaskByIdAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        return task;
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> CreateTask(TaskItemDto task)
    {
        var createdTask = await _adapter.CreateTaskAsync(task);
        
        // Create change event after successful operation
        await _changeEventService.CreateChangeEventAsync(
            projectId: createdTask.ProjectId,
            entityType: ChangeEventEntityType.Task,
            entityId: createdTask.Id,
            operation: ChangeEventOperation.Created,
            actorUserId: createdTask.AssignedTo);
        
        return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskItemDto task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        var success = await _adapter.UpdateTaskAsync(id, task);
        if (!success)
        {
            return NotFound();
        }

        // Create change event after successful operation
        await _changeEventService.CreateChangeEventAsync(
            projectId: task.ProjectId,
            entityType: ChangeEventEntityType.Task,
            entityId: task.Id,
            operation: ChangeEventOperation.Updated,
            actorUserId: task.AssignedTo);

        return NoContent();
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        // Get task info before deletion for the event
        var task = await _adapter.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        var success = await _adapter.DeleteTaskAsync(id);
        if (!success)
        {
            return NotFound();
        }

        // Create change event after successful operation
        await _changeEventService.CreateChangeEventAsync(
            projectId: task.ProjectId,
            entityType: ChangeEventEntityType.Task,
            entityId: id,
            operation: ChangeEventOperation.Deleted,
            actorUserId: task.AssignedTo);

        return NoContent();
    }
}
