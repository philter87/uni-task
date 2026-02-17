using Microsoft.AspNetCore.Mvc;
using UniTask.Api.DTOs;
using UniTask.Api.Services;

namespace UniTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChangeEventsController : ControllerBase
{
    private readonly IChangeEventService _changeEventService;

    public ChangeEventsController(IChangeEventService changeEventService)
    {
        _changeEventService = changeEventService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChangeEventDto>>> GetChangeEvents(
        [FromQuery] int? projectId = null,
        [FromQuery] int? sinceVersion = null)
    {
        var events = await _changeEventService.GetChangeEventsAsync(projectId, sinceVersion);
        var eventDtos = events.Select(e => new ChangeEventDto
        {
            Id = e.Id,
            EventId = e.EventId,
            ProjectId = e.ProjectId,
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            Operation = e.Operation,
            OccurredAt = e.OccurredAt,
            ActorUserId = e.ActorUserId,
            Version = e.Version,
            Payload = e.Payload
        });

        return Ok(eventDtos);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<ChangeEventDto>>> GetEntityChangeEvents(
        string entityType,
        int entityId)
    {
        var events = await _changeEventService.GetEntityChangeEventsAsync(entityType, entityId);
        var eventDtos = events.Select(e => new ChangeEventDto
        {
            Id = e.Id,
            EventId = e.EventId,
            ProjectId = e.ProjectId,
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            Operation = e.Operation,
            OccurredAt = e.OccurredAt,
            ActorUserId = e.ActorUserId,
            Version = e.Version,
            Payload = e.Payload
        });

        return Ok(eventDtos);
    }
}
