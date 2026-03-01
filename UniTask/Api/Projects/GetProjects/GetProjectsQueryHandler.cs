using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, IEnumerable<ProjectDto>>
{
    private readonly TaskDbContext _context;

    public GetProjectsQueryHandler(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Projects.AsQueryable();
        if (request.OrganisationId.HasValue)
            query = query.Where(p => p.OrganisationId == request.OrganisationId.Value);
        var projects = await query.ToListAsync(cancellationToken);

        return projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            ExternalId = p.ExternalId,
            Name = p.Name,
            Description = p.Description,
            OrganisationId = p.OrganisationId,
            Provider = p.Provider,
            TaskProviderAuthId = p.TaskProviderAuthId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }
}
