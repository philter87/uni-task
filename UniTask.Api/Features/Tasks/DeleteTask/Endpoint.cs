using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Infrastructure.Adapters;

namespace UniTask.Api.Features.Tasks.DeleteTask;

[ApiController]
[Route("api/tasks")]
public class DeleteTaskController : ControllerBase
{
    private readonly ITaskAdapter _adapter;

    public DeleteTaskController(ITaskAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var deleted = await _adapter.DeleteTaskAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
