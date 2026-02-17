using Microsoft.AspNetCore.Mvc;

namespace UniTask.Api.Features.Projects.GetAllProjects;

[ApiController]
[Route("api/projects")]
public class Endpoint : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Response>>> GetAllProjects()
    {
        // This will be implemented with a query in the future
        // For now, return empty list as placeholder
        return Ok(new List<Response>());
    }
}
