using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, IEnumerable<ProjectDto>>
{
    private readonly TaskDbContext _context;

    public GetProjectsQueryHandler(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _context.Projects.ToListAsync(cancellationToken);

        return projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            ExternalId = p.ExternalId,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }
}
