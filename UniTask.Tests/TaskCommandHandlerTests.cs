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
        var newTask = new TaskItemDto
        {
            Title = "Task to Change Status",
            Description = "Test Description",
            Priority = "Medium"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Arrange - Create a status
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var status = new Status
            {
                Name = "In Progress",
                Description = "Work in progress",
                Order = 1,
                ProjectId = null
            };
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
        var newTask = new TaskItemDto
        {
            Title = "Task to Assign",
            Description = "Test Description",
            Priority = "High"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Act - Assign member to task
        var assignMemberRequest = new { AssignedTo = "john.doe@example.com" };
        var response = await _client.PatchAsync($"/api/tasks/{createdTask.Id}/assign", 
            JsonContent.Create(assignMemberRequest));

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal("john.doe@example.com", updatedTask.AssignedTo);
    }

    [Fact]
    public async Task AssignMemberToTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var assignMemberRequest = new { AssignedTo = "john.doe@example.com" };

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
        var newTask = new TaskItemDto
        {
            Title = "Original Task Title",
            Description = "Original Description",
            Priority = "Low"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Act - Update task
        var updateRequest = new
        {
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = "Critical",
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedTo = "jane.smith@example.com"
        };

        var response = await _client.PutAsync($"/api/tasks/{createdTask.Id}/update", 
            JsonContent.Create(updateRequest));

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Task Title", updatedTask.Title);
        Assert.Equal("Updated Description", updatedTask.Description);
        Assert.Equal("Critical", updatedTask.Priority);
        Assert.Equal("jane.smith@example.com", updatedTask.AssignedTo);
    }

    [Fact]
    public async Task UpdateTaskWithCommand_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateRequest = new
        {
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = "High"
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
        var newTask = new TaskItemDto
        {
            Title = "Task for Label Test",
            Priority = "Medium"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Arrange - Create a label
        int labelId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var label = new Label
            {
                Name = "Bug",
                Color = "#FF0000"
            };
            db.Labels.Add(label);
            await db.SaveChangesAsync();
            labelId = label.Id;
        }

        // Act - Add label to task
        var response = await _client.PostAsync($"/api/tasks/{createdTask.Id}/labels/{labelId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Contains(updatedTask.Labels, l => l.Id == labelId && l.Name == "Bug");
    }

    [Fact]
    public async Task AddTaskLabel_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange - Create a label
        int labelId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var label = new Label
            {
                Name = "Feature",
                Color = "#00FF00"
            };
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
        var newTask = new TaskItemDto
        {
            Title = "Task for Label Test",
            Priority = "Medium"
        };

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
        var newTask = new TaskItemDto
        {
            Title = "Task for Label Removal Test",
            Priority = "Low"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Arrange - Create and add a label to the task
        int labelId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var label = new Label
            {
                Name = "Documentation",
                Color = "#0000FF"
            };
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
        var newTask = new TaskItemDto
        {
            Title = "Task for Duplicate Label Test",
            Priority = "Medium"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Arrange - Create a label
        int labelId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var label = new Label
            {
                Name = "Priority",
                Color = "#FFFF00"
            };
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
