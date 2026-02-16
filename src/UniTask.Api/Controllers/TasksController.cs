using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Data;
using UniTask.Api.Models;

namespace UniTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskDbContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TaskDbContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all tasks
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
        return await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Sprint)
            .Include(t => t.Labels)
            .ToListAsync();
    }

    /// <summary>
    /// Get a specific task by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Sprint)
            .Include(t => t.Labels)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

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
    public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskItem task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null)
        {
            return NotFound();
        }

        // Update only allowed fields to prevent overposting
        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.StatusId = task.StatusId;
        existingTask.OldStatus = task.OldStatus;
        existingTask.Priority = task.Priority;
        existingTask.DueDate = task.DueDate;
        existingTask.AssignedTo = task.AssignedTo;
        existingTask.ProjectId = task.ProjectId;
        existingTask.TaskTypeId = task.TaskTypeId;
        existingTask.SprintId = task.SprintId;
        existingTask.DurationMin = task.DurationMin;
        existingTask.RemainingMin = task.RemainingMin;
        existingTask.UpdatedAt = DateTime.UtcNow;
        // Note: CreatedAt, Source, and ExternalId are not updated to prevent overposting

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TaskExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> TaskExists(int id)
    {
        return await _context.Tasks.AnyAsync(e => e.Id == id);
    }
}
