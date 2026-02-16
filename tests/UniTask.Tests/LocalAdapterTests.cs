using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Adapters;
using UniTask.Api.Data;
using UniTask.Api.DTOs;
using UniTask.Api.Models;
using UniTask.Api.Services;
using Xunit;

namespace UniTask.Tests;

public class LocalAdapterTests : IDisposable
{
    private readonly TaskDbContext _context;
    private readonly LocalAdapter _adapter;
    private readonly IChangeEventService _changeEventService;

    public LocalAdapterTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new TaskDbContext(options);
        _changeEventService = new ChangeEventService(_context);
        _adapter = new LocalAdapter(_context, _changeEventService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsEmptyList_WhenNoTasks()
    {
        // Act
        var tasks = await _adapter.GetAllTasksAsync();

        // Assert
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task CreateTaskAsync_CreatesTaskAndReturnsDto()
    {
        // Arrange
        var taskDto = Any.TaskItemDto(
            title: "Test Task",
            description: "Test Description",
            priority: "High");

        // Act
        var result = await _adapter.CreateTaskAsync(taskDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("High", result.Priority);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.UpdatedAt > DateTime.MinValue);

        // Verify in database
        var dbTask = await _context.Tasks.FindAsync(result.Id);
        Assert.NotNull(dbTask);
        Assert.Equal("Test Task", dbTask.Title);
        Assert.Equal(TaskPriority.High, dbTask.Priority);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsDto_WhenTaskExists()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Existing Task",
            description: "Existing Description",
            priority: TaskPriority.Medium);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _adapter.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal("Existing Task", result.Title);
        Assert.Equal("Medium", result.Priority);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _adapter.GetTaskByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTaskAsync_UpdatesTask_WhenTaskExists()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Original Title",
            description: "Original Description",
            priority: TaskPriority.Low);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateDto = Any.TaskItemDto(
            id: task.Id,
            title: "Updated Title",
            description: "Updated Description",
            priority: "Critical");

        // Act
        var result = await _adapter.UpdateTaskAsync(task.Id, updateDto);

        // Assert
        Assert.True(result);

        // Verify in database
        var dbTask = await _context.Tasks.FindAsync(task.Id);
        Assert.NotNull(dbTask);
        Assert.Equal("Updated Title", dbTask.Title);
        Assert.Equal("Updated Description", dbTask.Description);
        Assert.Equal(TaskPriority.Critical, dbTask.Priority);
    }

    [Fact]
    public async Task UpdateTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateDto = Any.TaskItemDto(
            id: 999,
            title: "Updated Title",
            priority: "High");

        // Act
        var result = await _adapter.UpdateTaskAsync(999, updateDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteTaskAsync_DeletesTask_WhenTaskExists()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Task to Delete",
            priority: TaskPriority.Medium);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _adapter.DeleteTaskAsync(task.Id);

        // Assert
        Assert.True(result);

        // Verify deletion
        var dbTask = await _context.Tasks.FindAsync(task.Id);
        Assert.Null(dbTask);
    }

    [Fact]
    public async Task DeleteTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _adapter.DeleteTaskAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsTasksWithRelatedData()
    {
        // Arrange
        var project = Any.Project(name: "Test Project");
        _context.Projects.Add(project);

        var status = Any.Status(name: "In Progress", order: 1);
        _context.Statuses.Add(status);

        await _context.SaveChangesAsync();

        var task = Any.TaskItem(
            title: "Task with Relations",
            priority: TaskPriority.High,
            projectId: project.Id,
            statusId: status.Id);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var tasks = await _adapter.GetAllTasksAsync();

        // Assert
        var taskList = tasks.ToList();
        Assert.Single(taskList);
        
        var resultTask = taskList.First();
        Assert.NotNull(resultTask.Project);
        Assert.Equal("Test Project", resultTask.Project.Name);
        Assert.NotNull(resultTask.Status);
        Assert.Equal("In Progress", resultTask.Status.Name);
    }

    [Fact]
    public async Task CreateTaskAsync_ParsesPriorityCorrectly()
    {
        // Arrange
        var taskDto = Any.TaskItemDto(
            title: "Priority Test",
            priority: "low"); // Test case-insensitive parsing

        // Act
        var result = await _adapter.CreateTaskAsync(taskDto);

        // Assert
        var dbTask = await _context.Tasks.FindAsync(result.Id);
        Assert.NotNull(dbTask);
        Assert.Equal(TaskPriority.Low, dbTask.Priority);
    }

    [Fact]
    public async Task CreateTaskAsync_UsesDefaultPriorityForInvalidValue()
    {
        // Arrange
        var taskDto = Any.TaskItemDto(
            title: "Invalid Priority Test",
            priority: "InvalidPriority");

        // Act
        var result = await _adapter.CreateTaskAsync(taskDto);

        // Assert
        var dbTask = await _context.Tasks.FindAsync(result.Id);
        Assert.NotNull(dbTask);
        Assert.Equal(TaskPriority.Medium, dbTask.Priority); // Should default to Medium
    }

    [Fact]
    public async Task CreateTaskAsync_CreatesChangeEvent()
    {
        // Arrange
        var taskDto = Any.TaskItemDto(
            title: "Test Task",
            description: "Test Description",
            priority: "High");

        // Act
        var result = await _adapter.CreateTaskAsync(taskDto);

        // Assert - Verify change event was created
        var changeEvents = await _context.ChangeEvents
            .Where(e => e.EntityId == result.Id && e.EntityType == ChangeEventEntityType.Task)
            .ToListAsync();
        
        Assert.Single(changeEvents);
        var changeEvent = changeEvents.First();
        Assert.Equal(ChangeEventOperation.Created, changeEvent.Operation);
        Assert.NotNull(changeEvent.Payload);
    }

    [Fact]
    public async Task UpdateTaskAsync_CreatesChangeEvent()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Original Title",
            priority: TaskPriority.Low);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateDto = Any.TaskItemDto(
            id: task.Id,
            title: "Updated Title",
            priority: "High");

        // Act
        var result = await _adapter.UpdateTaskAsync(task.Id, updateDto);

        // Assert - Verify change event was created
        var changeEvents = await _context.ChangeEvents
            .Where(e => e.EntityId == task.Id && e.EntityType == ChangeEventEntityType.Task && e.Operation == ChangeEventOperation.Updated)
            .ToListAsync();
        
        Assert.Single(changeEvents);
        var changeEvent = changeEvents.First();
        Assert.Equal(ChangeEventOperation.Updated, changeEvent.Operation);
        Assert.NotNull(changeEvent.Payload);
    }

    [Fact]
    public async Task DeleteTaskAsync_CreatesChangeEvent()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Task to Delete",
            priority: TaskPriority.Medium);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        var taskId = task.Id;

        // Act
        var result = await _adapter.DeleteTaskAsync(taskId);

        // Assert - Verify change event was created
        var changeEvents = await _context.ChangeEvents
            .Where(e => e.EntityId == taskId && e.EntityType == ChangeEventEntityType.Task)
            .ToListAsync();
        
        Assert.Single(changeEvents);
        var changeEvent = changeEvents.First();
        Assert.Equal(ChangeEventOperation.Deleted, changeEvent.Operation);
        Assert.NotNull(changeEvent.Payload);
    }
}

