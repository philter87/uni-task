using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Commands;
using UniTask.Api.DTOs;

namespace UniTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectCommand command)
    {
        var project = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        // This will be implemented with a query in the future
        // For now, return NotFound as placeholder
        return NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAllProjects()
    {
        // This will be implemented with a query in the future
        // For now, return empty list as placeholder
        return Ok(new List<ProjectDto>());
    }
}
