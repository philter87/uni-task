using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;
using UniTask.Api.Shared.TaskProviderClients;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Commands.SyncTasks;
using UniTask.Api.Tasks.Queries.GetTasks;
using Xunit;

namespace UniTask.Tests;

public class SyncTasksCommandHandlerTests : IDisposable
{
    private readonly TaskDbContext _context;

    public SyncTasksCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TaskDbContext(options);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task SyncTasks_StoresTasksFromProvider_InDatabase()
    {
        // Arrange
        var project = Any.Project(name: "Test Project");
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var providerTask = new TaskItemDto { Id = 1, Title = "Provider Task", ExternalId = "ext-1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new StubTaskProviderClient(new[] { providerTask });
        var handler = new SyncTasksCommandHandler(_context, provider);

        // Act
        var result = await handler.Handle(new SyncTasksCommand { ProjectId = project.Id }, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Provider Task", resultList[0].Title);

        var dbTask = _context.Tasks.FirstOrDefault(t => t.ExternalId == "ext-1");
        Assert.NotNull(dbTask);
        Assert.Equal("Provider Task", dbTask.Title);
        Assert.Equal(project.Id, dbTask.ProjectId);
    }

    [Fact]
    public async Task SyncTasks_UpdatesExistingTask_WhenExternalIdMatches()
    {
        // Arrange
        var project = Any.Project();
        _context.Projects.Add(project);
        var existing = Any.TaskItem(title: "Old Title", projectId: project.Id);
        existing.ExternalId = "ext-42";
        _context.Tasks.Add(existing);
        await _context.SaveChangesAsync();

        var updatedTask = new TaskItemDto { Id = 0, Title = "New Title", ExternalId = "ext-42", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new StubTaskProviderClient(new[] { updatedTask });
        var handler = new SyncTasksCommandHandler(_context, provider);

        // Act
        await handler.Handle(new SyncTasksCommand { ProjectId = project.Id }, CancellationToken.None);

        // Assert
        var dbTask = _context.Tasks.First(t => t.ExternalId == "ext-42" && t.ProjectId == project.Id);
        Assert.Equal("New Title", dbTask.Title);
    }

    [Fact]
    public async Task SyncTasks_CreatesNewTask_WhenNoExternalIdMatch()
    {
        // Arrange
        var project = Any.Project();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var providerTask = new TaskItemDto { Id = 0, Title = "Brand New Task", ExternalId = "ext-99", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new StubTaskProviderClient(new[] { providerTask });
        var handler = new SyncTasksCommandHandler(_context, provider);

        // Act
        await handler.Handle(new SyncTasksCommand { ProjectId = project.Id }, CancellationToken.None);

        // Assert
        Assert.Equal(1, _context.Tasks.Count());
        Assert.Equal("Brand New Task", _context.Tasks.First().Title);
    }

    [Fact]
    public async Task SyncTasks_ReturnsEmptyList_WhenProviderReturnsNoTasks()
    {
        // Arrange
        var project = Any.Project();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var provider = new StubTaskProviderClient(Array.Empty<TaskItemDto>());
        var handler = new SyncTasksCommandHandler(_context, provider);

        // Act
        var result = await handler.Handle(new SyncTasksCommand { ProjectId = project.Id }, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Empty(_context.Tasks);
    }

    [Fact]
    public async Task SyncTasks_PassesProjectExternalId_ToProvider()
    {
        // Arrange
        var project = Any.Project();
        project.ExternalId = "owner/repo";
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var provider = new StubTaskProviderClient(Array.Empty<TaskItemDto>());
        var handler = new SyncTasksCommandHandler(_context, provider);

        // Act
        await handler.Handle(new SyncTasksCommand { ProjectId = project.Id }, CancellationToken.None);

        // Assert
        Assert.Equal("owner/repo", provider.LastQuery?.ExternalProjectId);
        Assert.Equal(project.Id, provider.LastQuery?.ProjectId);
    }
}

file class StubTaskProviderClient : ITaskProviderClient
{
    private readonly IEnumerable<TaskItemDto> _tasks;
    public GetTasksQuery? LastQuery { get; private set; }

    public StubTaskProviderClient(IEnumerable<TaskItemDto> tasks) => _tasks = tasks;

    public Task CreateProject(UniTask.Api.Projects.Events.ProjectCreatedEvent projectCreated) => Task.CompletedTask;
    public Task<IEnumerable<UniTask.Api.Projects.ProjectDto>> GetProjects(UniTask.Api.Projects.Queries.GetProjects.GetProjectsQuery getProjects) =>
        Task.FromResult(Enumerable.Empty<UniTask.Api.Projects.ProjectDto>());
    public Task CreateTask(UniTask.Api.Tasks.Events.TaskCreatedEvent taskCreated) => Task.CompletedTask;

    public Task<IEnumerable<TaskItemDto>> GetTasks(GetTasksQuery getTasks)
    {
        LastQuery = getTasks;
        return Task.FromResult(_tasks);
    }
}
