using System.Net;
using System.Net.Http.Json;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Organisations;

public class OrganisationsControllerTests : IDisposable
{
    private readonly AppFactory _factory;
    private readonly HttpClient _client;

    public OrganisationsControllerTests()
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
    public async Task CreateOrganisation_ReturnsOk()
    {
        // Arrange
        var command = Any.CreateOrganisationCommand();

        // Act
        var response = await _client.PostAsJsonAsync("/api/organisations", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrganisation_WithOnlyName_ReturnsOk()
    {
        // Arrange
        var command = Any.CreateOrganisationCommand(name: "Test Organisation");

        // Act
        var response = await _client.PostAsJsonAsync("/api/organisations", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
