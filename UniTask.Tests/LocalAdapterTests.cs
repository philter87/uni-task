using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Shared;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Adapters;
using UniTask.Api.Tasks.Commands.Create;
using UniTask.Api.Tasks.Commands.Delete;
using UniTask.Api.Tasks.Commands.Update;
using UniTask.Api.Tasks.Queries.GetTask;
using UniTask.Api.Tasks.Queries.GetTasks;
using Xunit;

namespace UniTask.Tests;

public class LocalAdapterTests : IDisposable
{
    private readonly TaskDbContext _context;
    private readonly LocalTasksAdapter _adapter;

    public LocalAdapterTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new TaskDbContext(options);
        _adapter = new LocalTasksAdapter(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllTasks_ReturnsEmptyList_WhenNoTasks()
    {
        // Act
        var tasks = await _adapter.Handle(new GetTasksQuery());

        // Assert
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task CreateTask_CreatesTaskAndReturnsEvent()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = 8.5
        };

        // Act
        var result = await _adapter.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TaskId > 0);
        Assert.Equal("Test Task", result.Title);
        Assert.True(result.CreatedAt > DateTime.MinValue);

        // Verify in database
        var dbTask = await _context.Tasks.FindAsync(result.TaskId);
        Assert.NotNull(dbTask);
        Assert.Equal("Test Task", dbTask.Title);
        Assert.Equal(8.5, dbTask.Priority);
    }

    [Fact]
    public async Task GetTaskById_ReturnsDto_WhenTaskExists()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Existing Task",
            description: "Existing Description",
            priority: 5.0);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _adapter.Handle(new GetTaskQuery { Id = task.Id });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal("Existing Task", result.Title);
        Assert.Equal(5.0, result.Priority);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNull_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _adapter.Handle(new GetTaskQuery { Id = 999 });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTask_UpdatesTask_WhenTaskExists()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Original Title",
            description: "Original Description",
            priority: 2.0);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var command = new UpdateTaskCommand
        {
            TaskId = task.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = 9.5
        };

        // Act
        var result = await _adapter.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.TaskId);
        Assert.Equal("Updated Title", result.Title);

        // Verify in database
        var dbTask = await _context.Tasks.FindAsync(task.Id);
        Assert.NotNull(dbTask);
        Assert.Equal("Updated Title", dbTask.Title);
        Assert.Equal("Updated Description", dbTask.Description);
        Assert.Equal(9.5, dbTask.Priority);
    }

    [Fact]
    public async Task UpdateTask_ThrowsException_WhenTaskDoesNotExist()
    {
        // Arrange
        var command = new UpdateTaskCommand
        {
            TaskId = 999,
            Title = "Updated Title",
            Priority = 8.0
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _adapter.Handle(command));
    }

    [Fact]
    public async Task DeleteTask_DeletesTask_WhenTaskExists()
    {
        // Arrange
        var task = Any.TaskItem(
            title: "Task to Delete",
            priority: 5.0);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _adapter.Handle(new DeleteTaskCommand { TaskId = task.Id });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.TaskId);

        // Verify deletion
        var dbTask = await _context.Tasks.FindAsync(task.Id);
        Assert.Null(dbTask);
    }

    [Fact]
    public async Task DeleteTask_ThrowsException_WhenTaskDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _adapter.Handle(new DeleteTaskCommand { TaskId = 999 }));
    }

    [Fact]
    public async Task GetAllTasks_ReturnsTasksWithRelatedData()
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
        var tasks = await _adapter.Handle(new GetTasksQuery());

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
    public async Task CreateTask_StoresPriorityCorrectly()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Priority Test",
            Priority = 2.5
        };

        // Act
        var result = await _adapter.Handle(command);

        // Assert
        var dbTask = await _context.Tasks.FindAsync(result.TaskId);
        Assert.NotNull(dbTask);
        Assert.Equal(2.5, dbTask.Priority);
    }

    [Fact]
    public async Task CreateTask_HandlesZeroPriority()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Zero Priority Test",
            Priority = 0
        };

        // Act
        var result = await _adapter.Handle(command);

        // Assert
        var dbTask = await _context.Tasks.FindAsync(result.TaskId);
        Assert.NotNull(dbTask);
        Assert.Equal(0, dbTask.Priority);
    }
}
