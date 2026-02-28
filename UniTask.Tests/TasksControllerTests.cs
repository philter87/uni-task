using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Commands.Create;
using UniTask.Api.Tasks.Commands.Update;
using UniTask.Api.Tasks.Events;
using UniTask.Tests.Utls;
using Xunit;

namespace UniTask.Tests;

public class TasksControllerTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TasksControllerTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task GetTasks_ReturnsEmptyList_WhenNoTasks()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItemDto>>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreatedTask()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = 5.0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var taskCreated = await response.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);
        Assert.Equal("Test Task", taskCreated.Title);
        Assert.NotEqual(Guid.Empty, taskCreated.TaskId);
        Assert.True(taskCreated.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task GetTask_ReturnsTask_WhenTaskExists()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Get Task Test",
            Priority = 8.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", command);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{taskCreated.TaskId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(task);
        Assert.Equal(taskCreated.TaskId, task.Id);
        Assert.Equal("Get Task Test", task.Title);
    }

    [Fact]
    public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_UpdatesTask_WhenTaskExists()
    {
        // Arrange
        var createCommand = new CreateTaskCommand
        {
            Title = "Original Title",
            Priority = 2.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createCommand);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

        var updateCommand = new UpdateTaskCommand
        {
            Title = "Updated Title",
            Priority = 8.0
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{taskCreated.TaskId}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var taskUpdated = await response.Content.ReadFromJsonAsync<TaskUpdatedEvent>();
        Assert.NotNull(taskUpdated);
        Assert.Equal("Updated Title", taskUpdated.Title);

        // Verify update via GET
        var getResponse = await _client.GetAsync($"/api/tasks/{taskCreated.TaskId}");
        var updatedTask = await getResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal(8.0, updatedTask.Priority);
    }

    [Fact]
    public async Task DeleteTask_DeletesTask_WhenTaskExists()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Task to Delete",
            Priority = 5.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", command);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{taskCreated.TaskId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var taskDeleted = await response.Content.ReadFromJsonAsync<TaskDeletedEvent>();
        Assert.NotNull(taskDeleted);
        Assert.Equal(taskCreated.TaskId, taskDeleted.TaskId);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/tasks/{taskCreated.TaskId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}