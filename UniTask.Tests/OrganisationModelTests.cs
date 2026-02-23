using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Shared;
using UniTask.Api.Users;
using Xunit;

namespace UniTask.Tests;

public class OrganisationModelTests : IDisposable
{
    private readonly TaskDbContext _context;

    public OrganisationModelTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task Organisation_CanBeCreated_WithRequiredFields()
    {
        // Arrange
        var organisation = Any.Organisation(name: "Test Org");

        // Act
        _context.Organisations.Add(organisation);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Organisations.FirstOrDefaultAsync(o => o.Id == organisation.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test Org", saved.Name);
    }

    [Fact]
    public async Task Organisation_CanHaveProjects()
    {
        // Arrange
        var organisation = Any.Organisation(name: "Org with Projects");
        _context.Organisations.Add(organisation);
        await _context.SaveChangesAsync();

        var project1 = Any.Project(name: "Project Alpha");
        var project2 = Any.Project(name: "Project Beta");
        project1.OrganisationId = organisation.Id;
        project2.OrganisationId = organisation.Id;

        // Act
        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Organisations
            .Include(o => o.Projects)
            .FirstOrDefaultAsync(o => o.Id == organisation.Id);

        Assert.NotNull(saved);
        Assert.Equal(2, saved.Projects.Count);
        Assert.Contains(saved.Projects, p => p.Name == "Project Alpha");
        Assert.Contains(saved.Projects, p => p.Name == "Project Beta");
    }

    [Fact]
    public async Task Organisation_CanHaveMembers()
    {
        // Arrange
        var organisation = Any.Organisation();
        var user = Any.User();
        _context.Organisations.Add(organisation);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var member = new OrganisationMember
        {
            OrganisationId = organisation.Id,
            UserId = user.Id,
            Role = "Admin"
        };

        // Act
        _context.OrganisationMembers.Add(member);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Organisations
            .Include(o => o.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(o => o.Id == organisation.Id);

        Assert.NotNull(saved);
        Assert.Single(saved.Members);
        Assert.Equal("Admin", saved.Members.First().Role);
        Assert.Equal(user.Id, saved.Members.First().UserId);
    }

    [Fact]
    public async Task Project_HasOptionalOrganisationReference()
    {
        // Arrange
        var organisation = Any.Organisation(name: "Parent Org");
        _context.Organisations.Add(organisation);
        await _context.SaveChangesAsync();

        var projectWithOrg = Any.Project(name: "Org Project");
        projectWithOrg.OrganisationId = organisation.Id;

        var projectWithoutOrg = Any.Project(name: "Standalone Project");

        // Act
        _context.Projects.AddRange(projectWithOrg, projectWithoutOrg);
        await _context.SaveChangesAsync();

        // Assert
        var savedWithOrg = await _context.Projects
            .Include(p => p.Organisation)
            .FirstOrDefaultAsync(p => p.Id == projectWithOrg.Id);

        var savedWithoutOrg = await _context.Projects
            .Include(p => p.Organisation)
            .FirstOrDefaultAsync(p => p.Id == projectWithoutOrg.Id);

        Assert.NotNull(savedWithOrg);
        Assert.NotNull(savedWithOrg.Organisation);
        Assert.Equal("Parent Org", savedWithOrg.Organisation.Name);

        Assert.NotNull(savedWithoutOrg);
        Assert.Null(savedWithoutOrg.Organisation);
    }

    [Fact]
    public async Task DeletingOrganisation_SetsProjectOrganisationIdToNull()
    {
        // Arrange
        var organisation = Any.Organisation(name: "Org to Delete");
        _context.Organisations.Add(organisation);
        await _context.SaveChangesAsync();

        var project = Any.Project(name: "Orphaned Project");
        project.OrganisationId = organisation.Id;
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        _context.Organisations.Remove(organisation);
        await _context.SaveChangesAsync();

        // Assert
        var savedProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);
        Assert.NotNull(savedProject);
        Assert.Null(savedProject.OrganisationId);
    }

    [Fact]
    public async Task Organisation_CanHaveTaskProviderAuths()
    {
        // Arrange
        var organisation = Any.Organisation();
        _context.Organisations.Add(organisation);
        await _context.SaveChangesAsync();

        var auth1 = Any.TaskProviderAuth(authenticationType: AuthenticationType.GitHubApp, authTypeId: "gh-app-id-123", organisationId: organisation.Id);
        var auth2 = Any.TaskProviderAuth(authenticationType: AuthenticationType.AzureAppRegistration, authTypeId: "azure-client-id", organisationId: organisation.Id);
        var auth3 = Any.TaskProviderAuth(authenticationType: AuthenticationType.AzureAppRegistration, authTypeId: "azure-client-id-2", organisationId: organisation.Id);

        // Act
        _context.TaskProviderAuths.AddRange(auth1, auth2, auth3);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Organisations
            .Include(o => o.Auths)
            .FirstOrDefaultAsync(o => o.Id == organisation.Id);

        Assert.NotNull(saved);
        Assert.Equal(3, saved.Auths.Count);
        Assert.Contains(saved.Auths, a => a.AuthenticationType == AuthenticationType.GitHubApp && a.AuthTypeId == "gh-app-id-123");
        Assert.Contains(saved.Auths, a => a.AuthenticationType == AuthenticationType.AzureAppRegistration && a.AuthTypeId == "azure-client-id");
        Assert.Contains(saved.Auths, a => a.AuthenticationType == AuthenticationType.AzureAppRegistration && a.AuthTypeId == "azure-client-id-2");
    }

    [Fact]
    public async Task DeletingOrganisation_CascadeDeletesTaskProviderAuths()
    {
        // Arrange
        var organisation = Any.Organisation();
        _context.Organisations.Add(organisation);
        await _context.SaveChangesAsync();

        var auth = Any.TaskProviderAuth(authenticationType: AuthenticationType.GitHubApp, authTypeId: "gh-app-id-123", organisationId: organisation.Id);
        _context.TaskProviderAuths.Add(auth);
        await _context.SaveChangesAsync();

        // Act
        _context.Organisations.Remove(organisation);
        await _context.SaveChangesAsync();

        // Assert
        var savedAuth = await _context.TaskProviderAuths.FirstOrDefaultAsync(a => a.Id == auth.Id);
        Assert.Null(savedAuth);
    }
}

public class UserModelTests : IDisposable
{
    private readonly TaskDbContext _context;

    public UserModelTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task User_CanBeCreated_WithRequiredFields()
    {
        // Arrange
        var user = Any.User(email: "test@example.com", username: "testuser");

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(saved);
        Assert.Equal("test@example.com", saved.Email);
        Assert.Equal("testuser", saved.UserName);
    }

    [Fact]
    public async Task User_CanBeProjectMember()
    {
        // Arrange
        var user = Any.User();
        var project = Any.Project();
        _context.Users.Add(user);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var member = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = user.Id,
            Role = "Developer",
            JoinedAt = DateTime.UtcNow
        };

        // Act
        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();

        // Assert
        var savedMember = await _context.ProjectMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == member.Id);

        Assert.NotNull(savedMember);
        Assert.Equal(user.Id, savedMember.UserId);
        Assert.Equal("Developer", savedMember.Role);
        Assert.NotNull(savedMember.User);
        Assert.Equal(user.Email, savedMember.User.Email);
    }

    [Fact]
    public async Task User_HasOptionalExternalIdAndDisplayName()
    {
        // Arrange
        var user = Any.User(externalId: "ext-123", displayName: "Test Display Name");

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(saved);
        Assert.Equal("ext-123", saved.ExternalId);
        Assert.Equal("Test Display Name", saved.DisplayName);
    }
}
