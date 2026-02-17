using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniTask.Api.Features.Projects.CreateProject;

[ApiController]
[Route("api/projects")]
public class CreateProjectController : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateProjectResponse>> CreateProject([FromBody] CreateProjectCommand command)
    {
        var project = await _mediator.Send(command);
        return CreatedAtRoute("GetProject", new { id = project.Id }, project);
    }
}
