using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Shared;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Projects.Commands;

public class CreateAuthTaskProviderHandlerTests
{
    private readonly AppFactory _factory = new AppFactory();
    private readonly IMediator _mediator;
    private readonly TaskDbContext _dbContext;

    public CreateAuthTaskProviderHandlerTests()
    {
        var scope = _factory.Services.CreateScope();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    }

    [Fact]
    public async Task Should_CreateAuthTaskProviderInDatabase_WhenCommandIsExecuted()
    {
        // Arrange
        var command = Any.CreateAuthTaskProviderCommand();

        // Act
        await _mediator.Send(command);

        // Assert
        Assert.Single(_dbContext.TaskProviderAuths);
    }

    [Fact]
    public async Task Should_StoreCorrectValues_WhenCommandIsExecuted()
    {
        // Arrange
        var command = Any.CreateAuthTaskProviderCommand();

        // Act
        await _mediator.Send(command);

        // Assert
        var auth = _dbContext.TaskProviderAuths.Single();
        Assert.Equal(command.Id, auth.Id);
        Assert.Equal(command.OrganisationId, auth.OrganisationId);
        Assert.Equal(command.AuthenticationType, auth.AuthenticationType);
        Assert.Equal(command.AuthTypeId, auth.AuthTypeId);
        Assert.Equal(command.SecretValue, auth.SecretValue);
    }
}
