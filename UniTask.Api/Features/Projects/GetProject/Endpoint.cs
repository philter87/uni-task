using Microsoft.AspNetCore.Mvc;

namespace UniTask.Api.Features.Projects.GetProject;

[ApiController]
[Route("api/projects")]
public class Endpoint : ControllerBase
{
    [HttpGet("{id}", Name = "GetProject")]
    public async Task<ActionResult<Response>> GetProject(int id)
    {
        // This will be implemented with a query in the future
        // For now, return NotFound as placeholder
        return NotFound();
    }
}
