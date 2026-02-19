using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Shared;
using UniTask.Api.Tasks;
using Xunit;

namespace UniTask.Tests;

public class TaskCommandHandlerTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TaskCommandHandlerTests()
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
    public async Task ChangeTaskStatus_UpdatesTaskStatus_WhenTaskExists()
    {
        // Arrange - Create a task
        var newTask = Any.TaskItemDto(
            title: "Task to Change Status",
            description: "Test Description",
            priority: 5.0);

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Arrange - Create a status
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var status = Any.Status(name: "In Progress", description: "Work in progress", order: 1);
            db.Statuses.Add(status);
            await db.SaveChangesAsync();

            // Act - Change task status
            var changeStatusRequest = new { StatusId = status.Id };
            var response = await _client.PatchAsync($"/api/tasks/{createdTask.Id}/status", 
                JsonContent.Create(changeStatusRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
            Assert.NotNull(updatedTask);
            Assert.Equal(status.Id, updatedTask.StatusId);
            Assert.Equal("In Progress", updatedTask.Status?.Name);
        }
    }

    [Fact]
    public async Task ChangeTaskStatus_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var changeStatusRequest = new { StatusId = 1 };

        // Act
        var response = await _client.PatchAsync("/api/tasks/999/status", 
            JsonContent.Create(changeStatusRequest));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignMemberToTask_UpdatesAssignedTo_WhenTaskExists()
    {
        // Arrange - Create a task
        var newTask = Any.TaskItemDto(
            title: "Task to Assign",
            description: "Test Description",
            priority: 8.0);

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Act - Assign member to task
        var assignedTo = Any.Email();
        var assignMemberRequest = new { AssignedTo = assignedTo };
        var response = await _client.PatchAsync($"/api/tasks/{createdTask.Id}/assign", 
            JsonContent.Create(assignMemberRequest));

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal(assignedTo, updatedTask.AssignedTo);
    }

    [Fact]
    public async Task AssignMemberToTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var assignMemberRequest = new { AssignedTo = Any.Email() };

        // Act
        var response = await _client.PatchAsync("/api/tasks/999/assign", 
            JsonContent.Create(assignMemberRequest));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTaskWithCommand_UpdatesTask_WhenTaskExists()
    {
        // Arrange - Create a task
        var newTask = Any.TaskItemDto(
            title: "Original Task Title",
            description: "Original Description",
            priority: 2.0);

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Act - Update task
        var dueDate = Any.DateTime(7, 14);
        var assignedTo = Any.Email();
        var updateRequest = new
        {
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = 9.5,
            DueDate = dueDate,
            AssignedTo = assignedTo
        };

        var response = await _client.PutAsync($"/api/tasks/{createdTask.Id}/update", 
            JsonContent.Create(updateRequest));

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Task Title", updatedTask.Title);
        Assert.Equal("Updated Description", updatedTask.Description);
        Assert.Equal(9.5, updatedTask.Priority);
        Assert.Equal(assignedTo, updatedTask.AssignedTo);
    }

    [Fact]
    public async Task UpdateTaskWithCommand_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateRequest = new
        {
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = 8.0
        };

        // Act
        var response = await _client.PutAsync("/api/tasks/999/update", 
            JsonContent.Create(updateRequest));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddTaskLabel_AddsLabelToTask_WhenBothExist()
    {
        // Arrange - Create a task
        var newTask = Any.TaskItemDto(
            title: "Task for Label Test",
            priority: 5.0);

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Arrange - Create a label
        int labelId;
        string labelName;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var label = Any.Label(name: "Bug");
            db.Labels.Add(label);
            await db.SaveChangesAsync();
            labelId = label.Id;
            labelName = label.Name;
        }

        // Act - Add label to task
        var response = await _client.PostAsync($"/api/tasks/{createdTask.Id}/labels/{labelId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Contains(updatedTask.Labels, l => l.Id == labelId && l.Name == labelName);
    }

    [Fact]
    public async Task AddTaskLabel_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange - Create a label
        int labelId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var label = Any.Label(name: "Feature");
            db.Labels.Add(label);
            await db.SaveChangesAsync();
            labelId = label.Id;
        }

        // Act
        var response = await _client.PostAsync($"/api/tasks/999/labels/{labelId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddTaskLabel_ReturnsNotFound_WhenLabelDoesNotExist()
    {
        // Arrange - Create a task
        var newTask = Any.TaskItemDto(
            title: "Task for Label Test",
            priority: 5.0);

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Act
        var response = await _client.PostAsync($"/api/tasks/{createdTask.Id}/labels/999", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveTaskLabel_RemovesLabelFromTask_WhenBothExist()
    {
        // Arrange - Create a task
        var newTask = Any.TaskItemDto(
            title: "Task for Label Removal Test",
            priority: 2.0);

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Arrange - Create and add a label to the task
        int labelId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var label = Any.Label(name: "Documentation");
            db.Labels.Add(label);
            await db.SaveChangesAsync();
            labelId = label.Id;

            // Add label to task
            var task = await db.Tasks.Include(t => t.Labels).FirstAsync(t => t.Id == createdTask.Id);
            task.Labels.Add(label);
            await db.SaveChangesAsync();
        }

        // Verify label was added
        var taskWithLabel = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        var taskDto = await taskWithLabel.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(taskDto);
        Assert.Contains(taskDto.Labels, l => l.Id == labelId);

        // Act - Remove label from task
        var response = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}/labels/{labelId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.DoesNotContain(updatedTask.Labels, l => l.Id == labelId);
    }

    [Fact]
    public async Task RemoveTaskLabel_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/999/labels/1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddTaskLabel_DoesNotAddDuplicateLabel_WhenLabelAlreadyExists()
    {
        // Arrange - Create a task
        var newTask = Any.TaskItemDto(
            title: "Task for Duplicate Label Test",
            priority: 5.0);

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Arrange - Create a label
        int labelId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var label = Any.Label(name: "Priority");
            db.Labels.Add(label);
            await db.SaveChangesAsync();
            labelId = label.Id;
        }

        // Act - Add label to task twice
        var response1 = await _client.PostAsync($"/api/tasks/{createdTask.Id}/labels/{labelId}", null);
        response1.EnsureSuccessStatusCode();

        var response2 = await _client.PostAsync($"/api/tasks/{createdTask.Id}/labels/{labelId}", null);
        response2.EnsureSuccessStatusCode();

        // Assert - Label should only appear once
        var updatedTask = await response2.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Single(updatedTask.Labels, l => l.Id == labelId);
    }
}
