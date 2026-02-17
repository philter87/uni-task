using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Adapters;
using UniTask.Api.DTOs;

namespace UniTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskAdapter _adapter;

    public TasksController(ITaskAdapter adapter)
    {
        _adapter = adapter;
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
}
