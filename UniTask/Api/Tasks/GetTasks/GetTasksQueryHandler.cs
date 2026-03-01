using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.GetTasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedResult<TaskItemDto>>
{
    private readonly TaskDbContext _context;

    public GetTasksQueryHandler(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TaskItemDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .Include(t => t.Status)
            .Include(t => t.Board)
            .Include(t => t.Labels).ThenInclude(l => l.LabelType)
            .Include(t => t.Tags)
            .AsQueryable();

        // Filters
        if (request.ProjectId.HasValue)
            query = query.Where(t => t.ProjectId == request.ProjectId.Value);

        if (request.OrganisationId.HasValue)
            query = query.Where(t => t.Project != null && t.Project.OrganisationId == request.OrganisationId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Title.Contains(request.Search)
                || (t.Description != null && t.Description.Contains(request.Search)));

        if (request.StatusIds is { Count: > 0 })
            query = query.Where(t => t.StatusId.HasValue && request.StatusIds.Contains(t.StatusId.Value));

        if (request.TaskTypeIds is { Count: > 0 })
            query = query.Where(t => t.TaskTypeId.HasValue && request.TaskTypeIds.Contains(t.TaskTypeId.Value));

        if (request.BoardIds is { Count: > 0 })
            query = query.Where(t => t.BoardId.HasValue && request.BoardIds.Contains(t.BoardId.Value));

        if (!string.IsNullOrWhiteSpace(request.AssignedTo))
            query = query.Where(t => t.AssignedTo != null && t.AssignedTo.Contains(request.AssignedTo));

        if (request.TagIds is { Count: > 0 })
            query = query.Where(t => t.Tags.Any(tag => request.TagIds.Contains(tag.Id)));

        // Sorting
        query = request.SortBy switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "priority" => request.SortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "status" => request.SortDescending ? query.OrderByDescending(t => t.Status!.Name) : query.OrderBy(t => t.Status!.Name),
            "createdAt" => request.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.UpdatedAt),
        };

        // Pagination
        var pageSize = Math.Min(Math.Max(request.PageSize, 1), 200);
        var page = Math.Max(request.Page, 1);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItemDto>
        {
            Items = items.Select(TaskItemMapper.MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
