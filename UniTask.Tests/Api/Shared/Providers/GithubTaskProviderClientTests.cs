using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Shared;
using UniTask.Api.Shared.TaskProviderClients;
using UniTask.Api.Tasks.Queries.GetTasks;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Shared.Providers;

public class GithubTaskProviderClientTests : IDisposable
{
    private readonly AppFactory _appFactory;
    private readonly GitHubTaskProviderClient _client;

    public GithubTaskProviderClientTests()
    {
        _appFactory = new AppFactory();
        _client = _appFactory.Services.GetRequiredService<GitHubTaskProviderClient>();
    }

    [Fact]
    public async Task Should_ReturnTasks_When_GetTasksIsCalled()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var query = new GetTasksQuery
        {
            ExternalProjectId = "org/repo",
            ProjectId = projectId
        };

        // Act
        var tasks = await _client.GetTasks(query);

        // Assert
        Assert.NotNull(tasks);
        var taskList = tasks.ToList();
        Assert.NotEmpty(taskList);
        Assert.Equal(5, taskList.Count);

        // Verify first task properties match JSON data
        var firstTask = taskList[0];
        Assert.Equal("1", firstTask.ExternalId);
        Assert.Equal("Add user authentication", firstTask.Title);
        Assert.Equal("Implement JWT authentication for API endpoints", firstTask.Description);
        Assert.Equal("developer1", firstTask.AssignedTo);
        Assert.Equal(projectId, firstTask.ProjectId);
        Assert.Equal(TaskProvider.GitHub, firstTask.Provider);
        Assert.Equal(new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc), firstTask.CreatedAt);
        Assert.Equal(new DateTime(2026, 2, 1, 14, 30, 0, DateTimeKind.Utc), firstTask.UpdatedAt);

        // Verify task with null assignee
        var thirdTask = taskList[2];
        Assert.Equal("3", thirdTask.ExternalId);
        Assert.Equal("Update documentation", thirdTask.Title);
        Assert.Null(thirdTask.AssignedTo);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_ExternalProjectIdIsEmpty()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            ExternalProjectId = string.Empty
        };

        // Act
        var tasks = await _client.GetTasks(query);

        // Assert
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_ExternalProjectIdIsNull()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            ExternalProjectId = null
        };

        // Act
        var tasks = await _client.GetTasks(query);

        // Assert
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    public void Dispose()
    {
        _appFactory?.Dispose();
    }
}