using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Projects.Commands.Create;
using UniTask.Api.Projects.Queries.GetProject;
using UniTask.Api.Projects.Queries.GetProjects;

namespace UniTask.Api.Projects;

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
    public async Task<ActionResult<ProjectCreatedEvent>> CreateProject([FromBody] CreateProjectCommand command)
    {
        var projectCreated = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProject), new { id = projectCreated.ProjectId }, projectCreated);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var project = await _mediator.Send(new GetProjectQuery { Id = id });
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAllProjects()
    {
        var projects = await _mediator.Send(new GetProjectsQuery());
        return Ok(projects);
    }
}
