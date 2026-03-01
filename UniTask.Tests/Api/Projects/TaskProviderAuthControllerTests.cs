using System.Net;
using System.Net.Http.Json;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Projects;

public class TaskProviderAuthControllerTests : IDisposable
{
    private readonly AppFactory _factory;
    private readonly HttpClient _client;

    public TaskProviderAuthControllerTests()
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
    public async Task CreateAuthTaskProvider_ReturnsOk()
    {
        // Arrange
        var command = Any.CreateTaskProviderAuthCommand();

        // Act
        var response = await _client.PostAsJsonAsync("/api/task-provider-auths", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
