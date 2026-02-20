using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects.Commands.Create;
using UniTask.Api.Projects.Queries.GetProject;
using UniTask.Api.Projects.Queries.GetProjects;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Adapters;

public class LocalProjectAdapter : IProjectAdapter
{
    private readonly TaskDbContext _context;

    public LocalProjectAdapter(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectCreatedEvent> Handle(CreateProjectCommand command)
    {
        var project = new Project
        {
            Name = command.Name,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return new ProjectCreatedEvent
        {
            ProjectId = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt
        };
    }

    public async Task<ProjectDto?> Handle(GetProjectQuery query)
    {
        var project = await _context.Projects.FindAsync(query.Id);

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

    public async Task<IEnumerable<ProjectDto>> Handle(GetProjectsQuery query)
    {
        var projects = await _context.Projects.ToListAsync();

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
