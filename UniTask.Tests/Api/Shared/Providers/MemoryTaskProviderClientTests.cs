using UniTask.Api.Projects.Events;
using UniTask.Api.Projects.Queries.GetProjects;
using UniTask.Api.Shared;
using UniTask.Api.Shared.TaskProviderClients;
using UniTask.Api.Tasks.Events;
using UniTask.Api.Tasks.Queries.GetTasks;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Shared.Providers;

public class MemoryTaskProviderClientTests
{
    private readonly MemoryTaskProviderClient _client = new();

    [Fact]
    public async Task CreateProject_StoresProjectDto()
    {
        var evt = new ProjectCreatedEvent
        {
            ProjectId = Guid.NewGuid(),
            Name = Any.String(),
            Description = Any.String(),
            CreatedAt = Any.DateTime()
        };

        await _client.CreateProject(evt);

        var projects = await _client.GetProjects(new GetProjectsQuery());
        var stored = Assert.Single(projects);
        Assert.Equal(evt.ProjectId, stored.Id);
        Assert.Equal(evt.Name, stored.Name);
        Assert.Equal(evt.Description, stored.Description);
        Assert.Equal(evt.CreatedAt, stored.CreatedAt);
    }

    [Fact]
    public async Task GetProjects_ReturnsEmpty_WhenNoProjectsCreated()
    {
        var projects = await _client.GetProjects(new GetProjectsQuery());
        Assert.Empty(projects);
    }

    [Fact]
    public async Task CreateProject_OverwritesExistingProject_WhenSameId()
    {
        var projectId = Guid.NewGuid();
        var first = new ProjectCreatedEvent { ProjectId = projectId, Name = Any.String(), CreatedAt = Any.DateTime() };
        var second = new ProjectCreatedEvent { ProjectId = projectId, Name = Any.String(), CreatedAt = Any.DateTime() };

        await _client.CreateProject(first);
        await _client.CreateProject(second);

        var projects = await _client.GetProjects(new GetProjectsQuery());
        var stored = Assert.Single(projects);
        Assert.Equal(second.Name, stored.Name);
    }

    [Fact]
    public async Task CreateTask_StoresTaskItemDto()
    {
        var evt = new TaskCreatedEvent
        {
            TaskId = Guid.NewGuid(),
            Title = Any.String(),
            CreatedAt = Any.DateTime()
        };

        await _client.CreateTask(evt);

        var tasks = await _client.GetTasks(new GetTasksQuery());
        var stored = Assert.Single(tasks);
        Assert.Equal(evt.TaskId, stored.Id);
        Assert.Equal(evt.Title, stored.Title);
        Assert.Equal(evt.CreatedAt, stored.CreatedAt);
    }

    [Fact]
    public async Task GetTasks_ReturnsEmpty_WhenNoTasksCreated()
    {
        var tasks = await _client.GetTasks(new GetTasksQuery());
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task CreateTask_OverwritesExistingTask_WhenSameId()
    {
        var taskId = Guid.NewGuid();
        var first = new TaskCreatedEvent { TaskId = taskId, Title = Any.String(), CreatedAt = Any.DateTime() };
        var second = new TaskCreatedEvent { TaskId = taskId, Title = Any.String(), CreatedAt = Any.DateTime() };

        await _client.CreateTask(first);
        await _client.CreateTask(second);

        var tasks = await _client.GetTasks(new GetTasksQuery());
        var stored = Assert.Single(tasks);
        Assert.Equal(second.Title, stored.Title);
    }

    [Fact]
    public async Task GetProjects_ReturnsAllCreatedProjects()
    {
        await _client.CreateProject(new ProjectCreatedEvent { ProjectId = Guid.NewGuid(), Name = Any.String(), CreatedAt = Any.DateTime() });
        await _client.CreateProject(new ProjectCreatedEvent { ProjectId = Guid.NewGuid(), Name = Any.String(), CreatedAt = Any.DateTime() });

        var projects = await _client.GetProjects(new GetProjectsQuery());
        Assert.Equal(2, projects.Count());
    }

    [Fact]
    public async Task GetTasks_ReturnsAllCreatedTasks()
    {
        await _client.CreateTask(new TaskCreatedEvent { TaskId = Guid.NewGuid(), Title = Any.String(), CreatedAt = Any.DateTime() });
        await _client.CreateTask(new TaskCreatedEvent { TaskId = Guid.NewGuid(), Title = Any.String(), CreatedAt = Any.DateTime() });

        var tasks = await _client.GetTasks(new GetTasksQuery());
        Assert.Equal(2, tasks.Count());
    }

    [Fact]
    public void ProjectCreatedEvent_ImplementsIProviderEvent()
    {
        var evt = new ProjectCreatedEvent { ProjectId = Guid.NewGuid(), Name = Any.String(), CreatedAt = Any.DateTime() };
        Assert.IsAssignableFrom<IProviderEvent>(evt);
        Assert.Equal(ChangeOrigin.Internal, evt.Origin);
        Assert.Equal(TaskProvider.Internal, evt.TaskProvider);
    }

    [Fact]
    public void TaskCreatedEvent_ImplementsIProviderEvent()
    {
        var evt = new TaskCreatedEvent { TaskId = Guid.NewGuid(), Title = Any.String(), CreatedAt = Any.DateTime() };
        Assert.IsAssignableFrom<IProviderEvent>(evt);
        Assert.Equal(ChangeOrigin.Internal, evt.Origin);
        Assert.Equal(TaskProvider.Internal, evt.TaskProvider);
    }
}
