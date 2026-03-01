using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Organisations.CreateTaskProviderAuth;

namespace UniTask.Api.Organisations;

[ApiController]
[Route("api/task-provider-auths")]
[Authorize]
public class TaskProviderAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskProviderAuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateTaskProviderAuth([FromBody] CreateTaskProviderAuthCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }
}
