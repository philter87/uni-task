using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Projects.Create;
using UniTask.Api.Projects.GetProject;
using UniTask.Api.Projects.GetProjects;

namespace UniTask.Api.Projects;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateProject([FromBody] CreateProjectCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProject), new { id }, id);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
    {
        var project = await _mediator.Send(new GetProjectQuery { Id = id });
        if (project == null)
            return NotFound();
        return Ok(project);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAllProjects([FromQuery] Guid? organisationId)
    {
        var projects = await _mediator.Send(new GetProjectsQuery { OrganisationId = organisationId });
        return Ok(projects);
    }
}
