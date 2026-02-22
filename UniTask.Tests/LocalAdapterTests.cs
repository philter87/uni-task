using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Shared;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Commands.Create;
using UniTask.Api.Tasks.Commands.Delete;
using UniTask.Api.Tasks.Commands.Update;
using UniTask.Api.Tasks.Events;
using UniTask.Api.Tasks.Queries.GetTask;
using UniTask.Api.Tasks.Queries.GetTasks;
using Xunit;

namespace UniTask.Tests;

public class TaskHandlerTests : IDisposable
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public TaskHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TaskDbContext(options);
        _publisher = new NoOpPublisher();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllTasks_ReturnsEmptyList_WhenNoTasks()
    {
        // Act
        var handler = new GetTasksQueryHandler(_context);
        var tasks = await handler.Handle(new GetTasksQuery(), CancellationToken.None);

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
        var handler = new CreateTaskCommandHandler(_context, _publisher);
        var result = await handler.Handle(command, CancellationToken.None);

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
        var handler = new GetTaskQueryHandler(_context);
        var result = await handler.Handle(new GetTaskQuery { Id = task.Id }, CancellationToken.None);

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
        var handler = new GetTaskQueryHandler(_context);
        var result = await handler.Handle(new GetTaskQuery { Id = 999 }, CancellationToken.None);

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
        var handler = new UpdateTaskCommandHandler(_context, _publisher);
        var result = await handler.Handle(command, CancellationToken.None);

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
        var handler = new UpdateTaskCommandHandler(_context, _publisher);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
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
        var handler = new DeleteTaskCommandHandler(_context, _publisher);
        var result = await handler.Handle(new DeleteTaskCommand { TaskId = task.Id }, CancellationToken.None);

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
        var handler = new DeleteTaskCommandHandler(_context, _publisher);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteTaskCommand { TaskId = 999 }, CancellationToken.None));
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
        var handler = new GetTasksQueryHandler(_context);
        var tasks = await handler.Handle(new GetTasksQuery(), CancellationToken.None);

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
        var handler = new CreateTaskCommandHandler(_context, _publisher);
        var result = await handler.Handle(command, CancellationToken.None);

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
        var handler = new CreateTaskCommandHandler(_context, _publisher);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var dbTask = await _context.Tasks.FindAsync(result.TaskId);
        Assert.NotNull(dbTask);
        Assert.Equal(0, dbTask.Priority);
    }
}

file class NoOpPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
}
