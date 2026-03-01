using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Organisations.Create;
using UniTask.Api.Organisations.Models;
using UniTask.Api.Projects;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations;

[ApiController]
[Route("api/organisations")]
[Authorize]
public class OrganisationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly TaskDbContext _context;

    public OrganisationsController(IMediator mediator, TaskDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateOrganisation([FromBody] CreateOrganisationCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<OrganisationDto>>> GetMyOrganisations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var userGuid = Guid.Parse(userId);
        var orgs = await _context.OrganisationMembers
            .Where(m => m.UserId == userGuid)
            .Include(m => m.Organisation)
            .Select(m => new OrganisationDto
            {
                Id = m.Organisation.Id,
                Name = m.Organisation.Name,
                IsPersonal = m.Organisation.IsPersonal,
                Provider = m.Organisation.Provider,
                Role = m.Role,
            })
            .ToListAsync();

        return Ok(orgs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrganisationDto>> GetOrganisation(Guid id)
    {
        var org = await _context.Organisations.FindAsync(id);
        if (org == null) return NotFound();

        return Ok(new OrganisationDto
        {
            Id = org.Id,
            Name = org.Name,
            IsPersonal = org.IsPersonal,
            Provider = org.Provider,
        });
    }

    [HttpGet("{id}/projects")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetOrganisationProjects(Guid id)
    {
        var projects = await _context.Projects
            .Where(p => p.OrganisationId == id)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ExternalId = p.ExternalId,
                OrganisationId = p.OrganisationId,
                Provider = p.Provider,
                TaskProviderAuthId = p.TaskProviderAuthId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
            })
            .ToListAsync();

        return Ok(projects);
    }
}

public class OrganisationDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsPersonal { get; set; }
    public UniTask.Api.Shared.TaskProvider? Provider { get; set; }
    public string? Role { get; set; }
}
