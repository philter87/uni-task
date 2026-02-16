using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UniTask.Api.Data;
using UniTask.Api.DTOs;
using UniTask.Api.Models;
using Xunit;

namespace UniTask.Tests;

public class ChangeEventsControllerTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ChangeEventsControllerTests()
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
    public async Task GetChangeEvents_ReturnsEmptyList_WhenNoEvents()
    {
        // Act
        var response = await _client.GetAsync("/api/changeevents");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<ChangeEventDto>>();
        Assert.NotNull(events);
        Assert.Empty(events);
    }

    [Fact]
    public async Task GetChangeEvents_ReturnsEvents_AfterTaskCreation()
    {
        // Arrange - Create a task which should trigger a change event
        var newTask = new TaskItemDto
        {
            Title = "Test Task for Change Event",
            Description = "Test Description",
            Priority = "Medium"
        };

        await _client.PostAsJsonAsync("/api/tasks", newTask);

        // Act
        var response = await _client.GetAsync("/api/changeevents");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<ChangeEventDto>>();
        Assert.NotNull(events);
        Assert.Single(events);
        
        var changeEvent = events.First();
        Assert.Equal(ChangeEventEntityType.Task, changeEvent.EntityType);
        Assert.Equal(ChangeEventOperation.Created, changeEvent.Operation);
        Assert.NotNull(changeEvent.EventId);
        Assert.NotNull(changeEvent.Payload);
    }

    [Fact]
    public async Task GetChangeEvents_ReturnsMultipleEvents_AfterMultipleOperations()
    {
        // Arrange - Create, update, and delete a task
        var newTask = new TaskItemDto
        {
            Title = "Test Task",
            Priority = "Low"
        };

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Update
        createdTask.Title = "Updated Task";
        await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);

        // Delete
        await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");

        // Act
        var response = await _client.GetAsync("/api/changeevents");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<ChangeEventDto>>();
        Assert.NotNull(events);
        Assert.Equal(3, events.Count);
        
        Assert.Equal(ChangeEventOperation.Created, events[0].Operation);
        Assert.Equal(ChangeEventOperation.Updated, events[1].Operation);
        Assert.Equal(ChangeEventOperation.Deleted, events[2].Operation);
    }

    [Fact]
    public async Task GetChangeEvents_FiltersEventsByVersion()
    {
        // Arrange - Create multiple tasks
        for (int i = 0; i < 3; i++)
        {
            var task = new TaskItemDto
            {
                Title = $"Task {i}",
                Priority = "Medium"
            };
            await _client.PostAsJsonAsync("/api/tasks", task);
        }

        // Get all events to find the last version
        var allEventsResponse = await _client.GetAsync("/api/changeevents");
        var allEvents = await allEventsResponse.Content.ReadFromJsonAsync<List<ChangeEventDto>>();
        Assert.NotNull(allEvents);
        var secondVersion = allEvents[1].Version;

        // Act - Get events since second version
        var response = await _client.GetAsync($"/api/changeevents?sinceVersion={secondVersion}");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<ChangeEventDto>>();
        Assert.NotNull(events);
        Assert.Single(events);
        Assert.True(events.First().Version > secondVersion);
    }

    [Fact]
    public async Task GetEntityChangeEvents_ReturnsEventsForSpecificEntity()
    {
        // Arrange - Create and update a task
        var newTask = new TaskItemDto
        {
            Title = "Test Task",
            Priority = "Low"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        createdTask.Title = "Updated Task";
        await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);

        // Create another task to ensure filtering works
        var otherTask = new TaskItemDto
        {
            Title = "Other Task",
            Priority = "High"
        };
        await _client.PostAsJsonAsync("/api/tasks", otherTask);

        // Act
        var response = await _client.GetAsync($"/api/changeevents/entity/{ChangeEventEntityType.Task}/{createdTask.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<ChangeEventDto>>();
        Assert.NotNull(events);
        Assert.Equal(2, events.Count);
        Assert.All(events, e => Assert.Equal(createdTask.Id, e.EntityId));
    }

    [Fact]
    public async Task ChangeEvents_HaveUniqueEventIds()
    {
        // Arrange - Create multiple tasks
        for (int i = 0; i < 5; i++)
        {
            var task = new TaskItemDto
            {
                Title = $"Task {i}",
                Priority = "Medium"
            };
            await _client.PostAsJsonAsync("/api/tasks", task);
        }

        // Act
        var response = await _client.GetAsync("/api/changeevents");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<ChangeEventDto>>();
        Assert.NotNull(events);
        
        var eventIds = events.Select(e => e.EventId).ToList();
        Assert.Equal(eventIds.Count, eventIds.Distinct().Count());
    }
}
