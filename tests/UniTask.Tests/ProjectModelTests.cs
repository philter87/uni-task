using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Data;
using UniTask.Api.Models;
using Xunit;

namespace UniTask.Tests;

public class ProjectModelTests : IDisposable
{
    private readonly TaskDbContext _context;

    public ProjectModelTests()
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
    public async Task Project_CanHaveTaskTypes()
    {
        // Arrange
        var project = Any.Project();

        var taskType1 = Any.TaskType(name: "Bug", description: "Bug task type");
        var taskType2 = Any.TaskType(name: "Feature", description: "Feature task type");

        // Act
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        taskType1.ProjectId = project.Id;
        taskType2.ProjectId = project.Id;
        _context.TaskTypes.Add(taskType1);
        _context.TaskTypes.Add(taskType2);
        await _context.SaveChangesAsync();

        // Assert
        var savedProject = await _context.Projects
            .Include(p => p.TaskTypes)
            .FirstOrDefaultAsync(p => p.Id == project.Id);

        Assert.NotNull(savedProject);
        Assert.Equal(2, savedProject.TaskTypes.Count);
        Assert.Contains(savedProject.TaskTypes, t => t.Name == "Bug");
        Assert.Contains(savedProject.TaskTypes, t => t.Name == "Feature");
    }

    [Fact]
    public async Task TaskType_HasProjectReference()
    {
        // Arrange
        var project = Any.Project(name: "Test Project");

        var taskType = Any.TaskType(name: "Story", description: "User story task type");

        // Act
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        taskType.ProjectId = project.Id;
        _context.TaskTypes.Add(taskType);
        await _context.SaveChangesAsync();

        // Assert
        var savedTaskType = await _context.TaskTypes
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskType.Id);

        Assert.NotNull(savedTaskType);
        Assert.NotNull(savedTaskType.Project);
        Assert.Equal(project.Id, savedTaskType.ProjectId);
        Assert.Equal("Test Project", savedTaskType.Project.Name);
    }

    [Fact]
    public async Task TaskType_CanBeGlobal_WithoutProject()
    {
        // Arrange
        var taskType = Any.TaskType(name: "Global Task Type", description: "Task type not associated with any project");

        // Act
        _context.TaskTypes.Add(taskType);
        await _context.SaveChangesAsync();

        // Assert
        var savedTaskType = await _context.TaskTypes
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskType.Id);

        Assert.NotNull(savedTaskType);
        Assert.Null(savedTaskType.ProjectId);
        Assert.Null(savedTaskType.Project);
    }

    [Fact]
    public async Task DeletingProject_SetsTaskTypeProjectIdToNull()
    {
        // Arrange
        var project = Any.Project(name: "Project to Delete");

        var taskType = Any.TaskType(name: "Task Type with Project");

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        taskType.ProjectId = project.Id;
        _context.TaskTypes.Add(taskType);
        await _context.SaveChangesAsync();

        // Act
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        // Assert
        var savedTaskType = await _context.TaskTypes
            .FirstOrDefaultAsync(t => t.Id == taskType.Id);

        Assert.NotNull(savedTaskType);
        Assert.Null(savedTaskType.ProjectId);
    }
}
