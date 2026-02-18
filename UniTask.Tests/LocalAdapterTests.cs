using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Shared;
using UniTask.Api.Shared.Adapters;
using UniTask.Api.Tasks;
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
        var taskDto = Any.TaskItemDto(
            title: "Test Task",
            description: "Test Description",
            priority: 8.5);

        // Act
        var result = await _adapter.CreateTaskAsync(taskDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(8.5, result.Priority);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.UpdatedAt > DateTime.MinValue);

        // Verify in database
        var dbTask = await _context.Tasks.FindAsync(result.Id);
        Assert.NotNull(dbTask);
        Assert.Equal("Test Task", dbTask.Title);
        Assert.Equal(8.5, dbTask.Priority);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsDto_WhenTaskExists()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Existing Task",
            description: "Existing Description",
            priority: 5.0);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _adapter.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal("Existing Task", result.Title);
        Assert.Equal(5.0, result.Priority);
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
            priority: 2.0);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateDto = Any.TaskItemDto(
            id: task.Id,
            title: "Updated Title",
            description: "Updated Description",
            priority: 9.5);

        // Act
        var result = await _adapter.UpdateTaskAsync(task.Id, updateDto);

        // Assert
        Assert.True(result);

        // Verify in database
        var dbTask = await _context.Tasks.FindAsync(task.Id);
        Assert.NotNull(dbTask);
        Assert.Equal("Updated Title", dbTask.Title);
        Assert.Equal("Updated Description", dbTask.Description);
        Assert.Equal(9.5, dbTask.Priority);
    }

    [Fact]
    public async Task UpdateTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateDto = Any.TaskItemDto(
            id: 999,
            title: "Updated Title",
            priority: 8.0);

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
            priority: 5.0);
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
            priority: 8.0,
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
    public async Task CreateTaskAsync_StoresPriorityCorrectly()
    {
        // Arrange
        var taskDto = Any.TaskItemDto(
            title: "Priority Test",
            priority: 2.5);

        // Act
        var result = await _adapter.CreateTaskAsync(taskDto);

        // Assert
        var dbTask = await _context.Tasks.FindAsync(result.Id);
        Assert.NotNull(dbTask);
        Assert.Equal(2.5, dbTask.Priority);
    }

    [Fact]
    public async Task CreateTaskAsync_HandlesZeroPriority()
    {
        // Arrange
        var taskDto = Any.TaskItemDto(
            title: "Zero Priority Test",
            priority: 0);

        // Act
        var result = await _adapter.CreateTaskAsync(taskDto);

        // Assert
        var dbTask = await _context.Tasks.FindAsync(result.Id);
        Assert.NotNull(dbTask);
        Assert.Equal(0, dbTask.Priority);
    }
}
