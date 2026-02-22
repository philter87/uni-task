using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Shared;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Commands.AssignMember;
using UniTask.Api.Tasks.Commands.ChangeStatus;
using UniTask.Api.Tasks.Commands.Create;
using UniTask.Api.Tasks.Commands.RemoveLabel;
using UniTask.Api.Tasks.Commands.AddLabel;
using UniTask.Api.Tasks.Commands.Update;
using UniTask.Api.Tasks.Events;
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
        var createCommand = new CreateTaskCommand
        {
            Title = "Task to Change Status",
            Description = "Test Description",
            Priority = 5.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createCommand);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

        // Arrange - Create a status
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var status = Any.Status(name: "In Progress", description: "Work in progress", order: 1);
            db.Statuses.Add(status);
            await db.SaveChangesAsync();

            // Act - Change task status
            var changeStatusRequest = new { StatusId = status.Id };
            var response = await _client.PatchAsync($"/api/tasks/{taskCreated.TaskId}/status", 
                JsonContent.Create(changeStatusRequest));

            // Assert
            response.EnsureSuccessStatusCode();
            var statusChanged = await response.Content.ReadFromJsonAsync<TaskStatusChangedEvent>();
            Assert.NotNull(statusChanged);
            Assert.Equal(taskCreated.TaskId, statusChanged.TaskId);
            Assert.Equal(status.Id, statusChanged.StatusId);
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
        var createCommand = new CreateTaskCommand
        {
            Title = "Task to Assign",
            Description = "Test Description",
            Priority = 8.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createCommand);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

        // Act - Assign member to task
        var assignedTo = Any.Email();
        var assignMemberRequest = new { AssignedTo = assignedTo };
        var response = await _client.PatchAsync($"/api/tasks/{taskCreated.TaskId}/assign", 
            JsonContent.Create(assignMemberRequest));

        // Assert
        response.EnsureSuccessStatusCode();
        var memberAssigned = await response.Content.ReadFromJsonAsync<MemberAssignedToTaskEvent>();
        Assert.NotNull(memberAssigned);
        Assert.Equal(taskCreated.TaskId, memberAssigned.TaskId);
        Assert.Equal(assignedTo, memberAssigned.AssignedTo);
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
    public async Task UpdateTask_UpdatesTask_WhenTaskExists()
    {
        // Arrange - Create a task
        var createCommand = new CreateTaskCommand
        {
            Title = "Original Task Title",
            Description = "Original Description",
            Priority = 2.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createCommand);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

        // Act - Update task
        var dueDate = Any.DateTime(7, 14);
        var assignedTo = Any.Email();
        var updateCommand = new UpdateTaskCommand
        {
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = 9.5,
            DueDate = dueDate,
            AssignedTo = assignedTo
        };

        var response = await _client.PutAsJsonAsync($"/api/tasks/{taskCreated.TaskId}", updateCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        var taskUpdated = await response.Content.ReadFromJsonAsync<TaskUpdatedEvent>();
        Assert.NotNull(taskUpdated);
        Assert.Equal(taskCreated.TaskId, taskUpdated.TaskId);
        Assert.Equal("Updated Task Title", taskUpdated.Title);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateCommand = new UpdateTaskCommand
        {
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = 8.0
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/tasks/999", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddTaskLabel_AddsLabelToTask_WhenBothExist()
    {
        // Arrange - Create a task
        var createCommand = new CreateTaskCommand
        {
            Title = "Task for Label Test",
            Priority = 5.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createCommand);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

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
        var response = await _client.PostAsync($"/api/tasks/{taskCreated.TaskId}/labels/{labelId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var labelAdded = await response.Content.ReadFromJsonAsync<TaskLabelAddedEvent>();
        Assert.NotNull(labelAdded);
        Assert.Equal(taskCreated.TaskId, labelAdded.TaskId);
        Assert.Equal(labelId, labelAdded.LabelId);

        // Verify label was added via GET
        var getResponse = await _client.GetAsync($"/api/tasks/{taskCreated.TaskId}");
        var taskDto = await getResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(taskDto);
        Assert.Contains(taskDto.Labels, l => l.Id == labelId && l.Name == labelName);
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
        var createCommand = new CreateTaskCommand
        {
            Title = "Task for Label Test",
            Priority = 5.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createCommand);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

        // Act
        var response = await _client.PostAsync($"/api/tasks/{taskCreated.TaskId}/labels/999", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveTaskLabel_RemovesLabelFromTask_WhenBothExist()
    {
        // Arrange - Create a task
        var createCommand = new CreateTaskCommand
        {
            Title = "Task for Label Removal Test",
            Priority = 2.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createCommand);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

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
            var task = await db.Tasks.Include(t => t.Labels).FirstAsync(t => t.Id == taskCreated.TaskId);
            task.Labels.Add(label);
            await db.SaveChangesAsync();
        }

        // Verify label was added
        var taskWithLabel = await _client.GetAsync($"/api/tasks/{taskCreated.TaskId}");
        var taskDto = await taskWithLabel.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(taskDto);
        Assert.Contains(taskDto.Labels, l => l.Id == labelId);

        // Act - Remove label from task
        var response = await _client.DeleteAsync($"/api/tasks/{taskCreated.TaskId}/labels/{labelId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var labelRemoved = await response.Content.ReadFromJsonAsync<TaskLabelRemovedEvent>();
        Assert.NotNull(labelRemoved);
        Assert.Equal(taskCreated.TaskId, labelRemoved.TaskId);
        Assert.Equal(labelId, labelRemoved.LabelId);

        // Verify label was removed via GET
        var getResponse = await _client.GetAsync($"/api/tasks/{taskCreated.TaskId}");
        var updatedTask = await getResponse.Content.ReadFromJsonAsync<TaskItemDto>();
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
        var createCommand = new CreateTaskCommand
        {
            Title = "Task for Duplicate Label Test",
            Priority = 5.0
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createCommand);
        var taskCreated = await createResponse.Content.ReadFromJsonAsync<TaskCreatedEvent>();
        Assert.NotNull(taskCreated);

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
        var response1 = await _client.PostAsync($"/api/tasks/{taskCreated.TaskId}/labels/{labelId}", null);
        response1.EnsureSuccessStatusCode();

        var response2 = await _client.PostAsync($"/api/tasks/{taskCreated.TaskId}/labels/{labelId}", null);
        response2.EnsureSuccessStatusCode();

        // Assert - Label should only appear once in GET response
        var getResponse = await _client.GetAsync($"/api/tasks/{taskCreated.TaskId}");
        var updatedTask = await getResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Single(updatedTask.Labels, l => l.Id == labelId);
    }
}
