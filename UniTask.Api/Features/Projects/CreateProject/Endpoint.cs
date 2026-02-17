using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniTask.Api.Features.Projects.CreateProject;

[ApiController]
[Route("api/projects")]
public class Endpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Response>> CreateProject([FromBody] Request command)
    {
        var project = await _mediator.Send(command);
        return CreatedAtRoute("GetProject", new { id = project.Id }, project);
    }
}
