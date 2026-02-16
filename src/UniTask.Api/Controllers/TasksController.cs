using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Adapters;
using UniTask.Api.DTOs;

namespace UniTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskAdapter _adapter;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskAdapter adapter, ILogger<TasksController> logger)
    {
        _adapter = adapter;
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

        return NoContent();
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var success = await _adapter.DeleteTaskAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
