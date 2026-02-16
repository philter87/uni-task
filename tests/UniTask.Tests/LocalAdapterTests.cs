using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Adapters;
using UniTask.Api.Data;
using UniTask.Api.DTOs;
using UniTask.Api.Models;
using Xunit;

namespace UniTask.Tests;

public class LocalAdapterTests : IDisposable
{
    private readonly TaskDbContext _context;
    private readonly LocalAdapter _adapter;

    public LocalAdapterTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new TaskDbContext(options);
        _adapter = new LocalAdapter(_context);
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
        var taskDto = new TaskItemDto
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = "High"
        };

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
        var task = new TaskItem
        {
            Title = "Existing Task",
            Description = "Existing Description",
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
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
        var task = new TaskItem
        {
            Title = "Original Title",
            Description = "Original Description",
            Priority = TaskPriority.Low,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateDto = new TaskItemDto
        {
            Id = task.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = "Critical"
        };

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
        var updateDto = new TaskItemDto
        {
            Id = 999,
            Title = "Updated Title",
            Priority = "High"
        };

        // Act
        var result = await _adapter.UpdateTaskAsync(999, updateDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteTaskAsync_DeletesTask_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Task to Delete",
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
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
        var project = new Project
        {
            Name = "Test Project",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);

        var status = new Status
        {
            Name = "In Progress",
            Order = 1
        };
        _context.Statuses.Add(status);

        await _context.SaveChangesAsync();

        var task = new TaskItem
        {
            Title = "Task with Relations",
            Priority = TaskPriority.High,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ProjectId = project.Id,
            StatusId = status.Id
        };
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
        var taskDto = new TaskItemDto
        {
            Title = "Priority Test",
            Priority = "low" // Test case-insensitive parsing
        };

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
        var taskDto = new TaskItemDto
        {
            Title = "Invalid Priority Test",
            Priority = "InvalidPriority"
        };

        // Act
        var result = await _adapter.CreateTaskAsync(taskDto);

        // Assert
        var dbTask = await _context.Tasks.FindAsync(result.Id);
        Assert.NotNull(dbTask);
        Assert.Equal(TaskPriority.Medium, dbTask.Priority); // Should default to Medium
    }
}
