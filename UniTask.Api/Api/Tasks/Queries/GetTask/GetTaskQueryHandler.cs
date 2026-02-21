using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Queries.GetTask;

public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, TaskItemDto?>
{
    private readonly TaskDbContext _context;

    public GetTaskQueryHandler(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItemDto?> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Board)
            .Include(t => t.Labels).ThenInclude(l => l.LabelType)
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        return task == null ? null : TaskItemMapper.MapToDto(task);
    }
}
