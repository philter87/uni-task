using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Projects.Commands.CreateAuthTaskProvider;

namespace UniTask.Api.Projects;

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
    public async Task CreateAuthTaskProvider([FromBody] CreateAuthTaskProviderCommand command)
    {
        await _mediator.Send(command);
    }
}
