using System.Net;
using System.Net.Http.Json;
using UniTask.Api.Projects.Commands.Create;
using UniTask.Api.Projects.Events;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Projects;

public class ProjectControllerTests : IDisposable
{
    private readonly AppFactory _factory;
    private readonly HttpClient _client;

    public ProjectControllerTests()
    {
        _factory = new AppFactory();
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
