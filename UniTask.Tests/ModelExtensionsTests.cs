using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Shared;
using UniTask.Api.Tasks;
using Xunit;

namespace UniTask.Tests;

public class ModelExtensionsTests : IDisposable
{
    private readonly TaskDbContext _context;

    public ModelExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task Project_CanHaveExternalId()
    {
        // Arrange
        var project = Any.Project();
        project.ExternalId = "ext-proj-123";

        // Act
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Assert
        var savedProject = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == project.Id);

        Assert.NotNull(savedProject);
        Assert.Equal("ext-proj-123", savedProject.ExternalId);
    }

    [Fact]
    public async Task TaskItem_ParentChild_Relationship()
    {
        // Arrange
        var parent = Any.TaskItem(title: "Parent Task");
        var child1 = Any.TaskItem(title: "Child Task 1");
        var child2 = Any.TaskItem(title: "Child Task 2");

        // Act
        _context.Tasks.Add(parent);
        await _context.SaveChangesAsync();

        child1.ParentId = parent.Id;
        child2.ParentId = parent.Id;
        _context.Tasks.AddRange(child1, child2);
        await _context.SaveChangesAsync();

        // Assert
        var savedParent = await _context.Tasks
            .Include(t => t.Children)
            .FirstOrDefaultAsync(t => t.Id == parent.Id);

        var savedChild1 = await _context.Tasks
            .Include(t => t.Parent)
            .FirstOrDefaultAsync(t => t.Id == child1.Id);

        Assert.NotNull(savedParent);
        Assert.Equal(2, savedParent.Children.Count);
        Assert.Contains(savedParent.Children, c => c.Title == "Child Task 1");
        Assert.Contains(savedParent.Children, c => c.Title == "Child Task 2");

        Assert.NotNull(savedChild1);
        Assert.NotNull(savedChild1.Parent);
        Assert.Equal("Parent Task", savedChild1.Parent.Title);
    }

    [Fact]
    public async Task TaskItem_CanHaveNullParent()
    {
        // Arrange
        var task = Any.TaskItem(title: "Independent Task");

        // Act
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Assert
        var savedTask = await _context.Tasks
            .Include(t => t.Parent)
            .FirstOrDefaultAsync(t => t.Id == task.Id);

        Assert.NotNull(savedTask);
        Assert.Null(savedTask.ParentId);
        Assert.Null(savedTask.Parent);
    }

    [Fact]
    public async Task TaskItemRelation_DependsOn_Relationship()
    {
        // Arrange
        var task1 = Any.TaskItem(title: "Task 1");
        var task2 = Any.TaskItem(title: "Task 2");

        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        var relation = new TaskItemRelation
        {
            FromTaskId = task1.Id,
            ToTaskId = task2.Id,
            FromRelationType = TaskRelationType.DependsOn,
            ToRelationType = TaskRelationType.IsBlockedBy,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.TaskItemRelations.Add(relation);
        await _context.SaveChangesAsync();

        // Assert
        var savedRelation = await _context.TaskItemRelations
            .Include(r => r.FromTask)
            .Include(r => r.ToTask)
            .FirstOrDefaultAsync(r => r.Id == relation.Id);

        Assert.NotNull(savedRelation);
        Assert.Equal("Task 1", savedRelation.FromTask.Title);
        Assert.Equal("Task 2", savedRelation.ToTask.Title);
        Assert.Equal(TaskRelationType.DependsOn, savedRelation.FromRelationType);
        Assert.Equal(TaskRelationType.IsBlockedBy, savedRelation.ToRelationType);
    }

    [Fact]
    public async Task TaskItemRelation_MultipleTasks_CanHaveMultipleRelations()
    {
        // Arrange
        var task1 = Any.TaskItem(title: "Task 1");
        var task2 = Any.TaskItem(title: "Task 2");
        var task3 = Any.TaskItem(title: "Task 3");

        _context.Tasks.AddRange(task1, task2, task3);
        await _context.SaveChangesAsync();

        var relation1 = new TaskItemRelation
        {
            FromTaskId = task1.Id,
            ToTaskId = task2.Id,
            FromRelationType = TaskRelationType.DependsOn,
            ToRelationType = TaskRelationType.IsBlockedBy,
            CreatedAt = DateTime.UtcNow
        };

        var relation2 = new TaskItemRelation
        {
            FromTaskId = task1.Id,
            ToTaskId = task3.Id,
            FromRelationType = TaskRelationType.Mentions,
            ToRelationType = TaskRelationType.IsMentionedBy,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.TaskItemRelations.AddRange(relation1, relation2);
        await _context.SaveChangesAsync();

        // Assert
        var savedTask1 = await _context.Tasks
            .Include(t => t.RelationsFrom)
                .ThenInclude(r => r.ToTask)
            .FirstOrDefaultAsync(t => t.Id == task1.Id);

        Assert.NotNull(savedTask1);
        Assert.Equal(2, savedTask1.RelationsFrom.Count);
        Assert.Contains(savedTask1.RelationsFrom, r => r.ToTask.Title == "Task 2");
        Assert.Contains(savedTask1.RelationsFrom, r => r.ToTask.Title == "Task 3");
    }

    [Fact]
    public async Task TaskItemRelation_BidirectionalRelations()
    {
        // Arrange
        var task1 = Any.TaskItem(title: "Task A");
        var task2 = Any.TaskItem(title: "Task B");

        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        var relation = new TaskItemRelation
        {
            FromTaskId = task1.Id,
            ToTaskId = task2.Id,
            FromRelationType = TaskRelationType.Blocks,
            ToRelationType = TaskRelationType.IsBlockedBy,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.TaskItemRelations.Add(relation);
        await _context.SaveChangesAsync();

        // Assert - Check from both sides
        var task1WithRelations = await _context.Tasks
            .Include(t => t.RelationsFrom)
            .FirstOrDefaultAsync(t => t.Id == task1.Id);

        var task2WithRelations = await _context.Tasks
            .Include(t => t.RelationsTo)
            .FirstOrDefaultAsync(t => t.Id == task2.Id);

        Assert.NotNull(task1WithRelations);
        Assert.Single(task1WithRelations.RelationsFrom);
        Assert.Equal(TaskRelationType.Blocks, task1WithRelations.RelationsFrom.First().FromRelationType);

        Assert.NotNull(task2WithRelations);
        Assert.Single(task2WithRelations.RelationsTo);
        Assert.Equal(TaskRelationType.IsBlockedBy, task2WithRelations.RelationsTo.First().ToRelationType);
    }

    [Fact]
    public async Task Sprint_CanHaveExternalId()
    {
        // Arrange
        var project = Any.Project();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var sprint = Any.Sprint(projectId: project.Id);
        sprint.ExternalId = "ext-sprint-456";

        // Act
        _context.Sprints.Add(sprint);
        await _context.SaveChangesAsync();

        // Assert
        var savedSprint = await _context.Sprints
            .FirstOrDefaultAsync(s => s.Id == sprint.Id);

        Assert.NotNull(savedSprint);
        Assert.Equal("ext-sprint-456", savedSprint.ExternalId);
    }

    [Fact]
    public async Task TaskType_CanHaveExternalId()
    {
        // Arrange
        var taskType = Any.TaskType();
        taskType.ExternalId = "ext-type-789";

        // Act
        _context.TaskTypes.Add(taskType);
        await _context.SaveChangesAsync();

        // Assert
        var savedTaskType = await _context.TaskTypes
            .FirstOrDefaultAsync(t => t.Id == taskType.Id);

        Assert.NotNull(savedTaskType);
        Assert.Equal("ext-type-789", savedTaskType.ExternalId);
    }

    [Fact]
    public async Task Status_CanHaveExternalId()
    {
        // Arrange
        var status = Any.Status();
        status.ExternalId = "ext-status-abc";

        // Act
        _context.Statuses.Add(status);
        await _context.SaveChangesAsync();

        // Assert
        var savedStatus = await _context.Statuses
            .FirstOrDefaultAsync(s => s.Id == status.Id);

        Assert.NotNull(savedStatus);
        Assert.Equal("ext-status-abc", savedStatus.ExternalId);
    }

    [Fact]
    public async Task Label_CanHaveExternalId()
    {
        // Arrange
        var label = new Label
        {
            Name = "Test Label",
            ExternalId = "ext-label-xyz",
            Color = "#FF0000"
        };

        // Act
        _context.Labels.Add(label);
        await _context.SaveChangesAsync();

        // Assert
        var savedLabel = await _context.Labels
            .FirstOrDefaultAsync(l => l.Id == label.Id);

        Assert.NotNull(savedLabel);
        Assert.Equal("ext-label-xyz", savedLabel.ExternalId);
    }

    [Fact]
    public async Task DeletingTask_DeletesRelations()
    {
        // Arrange
        var task1 = Any.TaskItem(title: "Task 1");
        var task2 = Any.TaskItem(title: "Task 2");

        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        var relation = new TaskItemRelation
        {
            FromTaskId = task1.Id,
            ToTaskId = task2.Id,
            FromRelationType = TaskRelationType.RelatesTo,
            ToRelationType = TaskRelationType.RelatesTo,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskItemRelations.Add(relation);
        await _context.SaveChangesAsync();

        // Act
        _context.Tasks.Remove(task1);
        await _context.SaveChangesAsync();

        // Assert
        var remainingRelations = await _context.TaskItemRelations
            .Where(r => r.Id == relation.Id)
            .ToListAsync();

        Assert.Empty(remainingRelations);
    }
}
