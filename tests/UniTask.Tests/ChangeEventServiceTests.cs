using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Data;
using UniTask.Api.Models;
using UniTask.Api.Services;
using Xunit;

namespace UniTask.Tests;

public class ChangeEventServiceTests : IDisposable
{
    private readonly TaskDbContext _context;
    private readonly ChangeEventService _service;

    public ChangeEventServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new TaskDbContext(options);
        _service = new ChangeEventService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateChangeEventAsync_CreatesEventSuccessfully()
    {
        // Act
        var result = await _service.CreateChangeEventAsync(
            projectId: 1,
            entityType: ChangeEventEntityType.Task,
            entityId: 100,
            operation: ChangeEventOperation.Created,
            actorUserId: "user123");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.NotNull(result.EventId);
        Assert.Equal(1, result.ProjectId);
        Assert.Equal(ChangeEventEntityType.Task, result.EntityType);
        Assert.Equal(100, result.EntityId);
        Assert.Equal(ChangeEventOperation.Created, result.Operation);
        Assert.Equal("user123", result.ActorUserId);
        Assert.True(result.Version > 0);
        Assert.Null(result.Payload); // Events are thin - no payload
    }

    [Fact]
    public async Task GetChangeEventsAsync_ReturnsAllEvents()
    {
        // Arrange
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 1, ChangeEventOperation.Created, "user1");
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 2, ChangeEventOperation.Created, "user2");

        // Act
        var events = await _service.GetChangeEventsAsync();

        // Assert
        Assert.NotNull(events);
        Assert.Equal(2, events.Count());
    }

    [Fact]
    public async Task GetChangeEventsAsync_FiltersEventsByProject()
    {
        // Arrange
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 1, ChangeEventOperation.Created, "user1");
        await _service.CreateChangeEventAsync(2, ChangeEventEntityType.Task, 2, ChangeEventOperation.Created, "user2");
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 3, ChangeEventOperation.Created, "user1");

        // Act
        var events = await _service.GetChangeEventsAsync(projectId: 1);

        // Assert
        Assert.NotNull(events);
        Assert.Equal(2, events.Count());
        Assert.All(events, e => Assert.Equal(1, e.ProjectId));
    }

    [Fact]
    public async Task GetChangeEventsAsync_FiltersEventsByVersion()
    {
        // Arrange
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 1, ChangeEventOperation.Created, "user1");
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 2, ChangeEventOperation.Created, "user2");
        var lastEvent = await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 3, ChangeEventOperation.Created, "user1");

        // Act
        var events = await _service.GetChangeEventsAsync(sinceVersion: lastEvent.Version - 1);

        // Assert
        Assert.NotNull(events);
        Assert.Single(events);
        Assert.Equal(lastEvent.Version, events.First().Version);
    }

    [Fact]
    public async Task GetEntityChangeEventsAsync_ReturnsEventsForSpecificEntity()
    {
        // Arrange
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 1, ChangeEventOperation.Created, "user1");
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 1, ChangeEventOperation.Updated, "user1");
        await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 2, ChangeEventOperation.Created, "user2");

        // Act
        var events = await _service.GetEntityChangeEventsAsync(ChangeEventEntityType.Task, 1);

        // Assert
        Assert.NotNull(events);
        Assert.Equal(2, events.Count());
        Assert.All(events, e => Assert.Equal(1, e.EntityId));
    }

    [Fact]
    public async Task CreateChangeEventAsync_GeneratesUniqueEventIds()
    {
        // Act
        var event1 = await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 1, ChangeEventOperation.Created, "user1");
        var event2 = await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 2, ChangeEventOperation.Created, "user1");

        // Assert
        Assert.NotEqual(event1.EventId, event2.EventId);
    }

    [Fact]
    public async Task CreateChangeEventAsync_IncrementsVersions()
    {
        // Act
        var event1 = await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 1, ChangeEventOperation.Created, "user1");
        var event2 = await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 2, ChangeEventOperation.Created, "user1");
        var event3 = await _service.CreateChangeEventAsync(1, ChangeEventEntityType.Task, 3, ChangeEventOperation.Created, "user1");

        // Assert
        Assert.True(event2.Version > event1.Version);
        Assert.True(event3.Version > event2.Version);
    }
}
