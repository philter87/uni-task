using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Queries.GetProject;

public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, ProjectDto?>
{
    private readonly TaskDbContext _context;

    public GetProjectQueryHandler(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto?> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects.FindAsync(request.Id);

        if (project == null)
        {
            return null;
        }

        return new ProjectDto
        {
            Id = project.Id,
            ExternalId = project.ExternalId,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}
