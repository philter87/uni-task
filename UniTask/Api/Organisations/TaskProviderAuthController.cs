using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Organisations.CreateTaskProviderAuth;

namespace UniTask.Api.Organisations;

[ApiController]
[Route("api/task-provider-auths")]
public class TaskProviderAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskProviderAuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task CreateTaskProviderAuth([FromBody] CreateTaskProviderAuthCommand command)
    {
        await _mediator.Send(command);
    }
}
