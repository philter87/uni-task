using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Projects.Commands.Create;
using UniTask.Api.Projects.Queries.GetProject;
using UniTask.Api.Projects.Queries.GetProjects;

namespace UniTask.Api.Projects;

[ApiController]
[Route("api/projects")]
public class ProjectController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task CreateProject([FromBody] CreateProjectCommand command)
    {
        await _mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
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
