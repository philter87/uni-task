using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using UniTask.Api.Commands;
using UniTask.Api.DTOs;
using Xunit;

namespace UniTask.Tests;

public class ProjectsControllerTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProjectsControllerTests()
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
    public async Task CreateProject_ReturnsCreatedProject()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test project description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.NotNull(createdProject);
        Assert.Equal("Test Project", createdProject.Name);
        Assert.Equal("Test project description", createdProject.Description);
        Assert.True(createdProject.Id > 0);
        Assert.True(createdProject.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task CreateProject_CreatesChangeEvent()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Project with Event",
            Description = "This should create a change event"
        };

        // Act - Create the project
        var createResponse = await _client.PostAsJsonAsync("/api/projects", command);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.NotNull(createdProject);

        // Get change events
        var eventsResponse = await _client.GetAsync("/api/changeevents");
        
        // Assert
        eventsResponse.EnsureSuccessStatusCode();
        var events = await eventsResponse.Content.ReadFromJsonAsync<List<ChangeEventDto>>();
        Assert.NotNull(events);
        
        var projectEvent = events.FirstOrDefault(e => 
            e.EntityType == "Project" && 
            e.EntityId == createdProject.Id &&
            e.Operation == "Created");
        
        Assert.NotNull(projectEvent);
        Assert.Equal(createdProject.Id, projectEvent.ProjectId);
    }

    [Fact]
    public async Task CreateProject_WithOnlyName_ReturnsCreatedProject()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Minimal Project"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.NotNull(createdProject);
        Assert.Equal("Minimal Project", createdProject.Name);
        Assert.Null(createdProject.Description);
        Assert.True(createdProject.Id > 0);
    }
}
