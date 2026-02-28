using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UniTask.Api.Tasks.Commands.AddLabel;
using UniTask.Api.Tasks.Commands.AssignMember;
using UniTask.Api.Tasks.Commands.ChangeStatus;
using UniTask.Api.Tasks.Commands.Create;
using UniTask.Api.Tasks.Commands.Delete;
using UniTask.Api.Tasks.Commands.RemoveLabel;
using UniTask.Api.Tasks.Commands.SyncTasks;
using UniTask.Api.Tasks.Commands.Update;
using UniTask.Api.Tasks.Events;
using UniTask.Api.Tasks.Queries.GetTask;
using UniTask.Api.Tasks.Queries.GetTasks;

namespace UniTask.Api.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
    {
        var tasks = await _mediator.Send(new GetTasksQuery());
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDto>> GetTask(Guid id)
    {
        var task = await _mediator.Send(new GetTaskQuery { Id = id });
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    [HttpPost("sync")]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> SyncTasks([FromBody] SyncTasksCommand command)
    {
        var tasks = await _mediator.Send(command);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TaskCreatedEvent>> CreateTask([FromBody] CreateTaskCommand command)
    {
        var taskCreated = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id = taskCreated.TaskId }, taskCreated);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskUpdatedEvent>> UpdateTask(Guid id, [FromBody] UpdateTaskCommand command)
    {
        command.TaskId = id;
        try
        {
            var taskUpdated = await _mediator.Send(command);
            return Ok(taskUpdated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<TaskDeletedEvent>> DeleteTask(Guid id)
    {
        try
        {
            var taskDeleted = await _mediator.Send(new DeleteTaskCommand { TaskId = id });
            return Ok(taskDeleted);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TaskStatusChangedEvent>> ChangeTaskStatus(Guid id, [FromBody] ChangeTaskStatusRequest request)
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
    public async Task<ActionResult<MemberAssignedToTaskEvent>> AssignMemberToTask(Guid id, [FromBody] AssignMemberRequest request)
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

    [HttpPost("{id}/labels/{labelId}")]
    public async Task<ActionResult<TaskLabelAddedEvent>> AddTaskLabel(Guid id, Guid labelId)
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
    public async Task<ActionResult<TaskLabelRemovedEvent>> RemoveTaskLabel(Guid id, Guid labelId)
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
    public Guid StatusId { get; set; }
}

public class AssignMemberRequest
{
    public required string AssignedTo { get; set; }
}
