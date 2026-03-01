using MediatR;

namespace UniTask.Api.Tasks.GetTasks;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

public class GetTasksQuery : IRequest<PagedResult<TaskItemDto>>
{
    // Used internally by sync (not exposed to API)
    public string? ExternalProjectId { get; set; }

    // Filters
    public Guid? ProjectId { get; set; }
    public Guid? OrganisationId { get; set; }
    public string? Search { get; set; }
    public List<Guid>? TagIds { get; set; }
    public List<Guid>? StatusIds { get; set; }
    public List<Guid>? TaskTypeIds { get; set; }
    public List<Guid>? BoardIds { get; set; }
    public string? AssignedTo { get; set; }

    // Sort
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
