using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Queries.GetTasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IEnumerable<TaskItemDto>>
{
    private readonly TaskDbContext _context;

    public GetTasksQueryHandler(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItemDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Board)
            .Include(t => t.Labels).ThenInclude(l => l.LabelType)
            .Include(t => t.Tags)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(t => t.ProjectId == request.ProjectId.Value);

        var tasks = await query.ToListAsync(cancellationToken);
        return tasks.Select(TaskItemMapper.MapToDto);
    }
}
