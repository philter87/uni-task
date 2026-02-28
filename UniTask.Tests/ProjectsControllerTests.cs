using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using UniTask.Api.Projects;
using UniTask.Api.Projects.Commands.Create;
using UniTask.Api.Projects.Events;
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
        var projectCreated = await response.Content.ReadFromJsonAsync<ProjectCreatedEvent>();
        Assert.NotNull(projectCreated);
        Assert.Equal("Test Project", projectCreated.Name);
        Assert.Equal("Test project description", projectCreated.Description);
        Assert.NotEqual(Guid.Empty, projectCreated.ProjectId);
        Assert.True(projectCreated.CreatedAt > DateTime.MinValue);
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
        var projectCreated = await response.Content.ReadFromJsonAsync<ProjectCreatedEvent>();
        Assert.NotNull(projectCreated);
        Assert.Equal("Minimal Project", projectCreated.Name);
        Assert.Null(projectCreated.Description);
        Assert.NotEqual(Guid.Empty, projectCreated.ProjectId);
    }
}
