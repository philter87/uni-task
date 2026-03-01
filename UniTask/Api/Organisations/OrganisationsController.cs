using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Organisations.Commands.Create;

namespace UniTask.Api.Organisations;

[ApiController]
[Route("api/organisations")]
public class OrganisationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganisationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task CreateOrganisation([FromBody] CreateOrganisationCommand command)
    {
        await _mediator.Send(command);
    }
}
