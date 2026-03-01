using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Shared;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Organisations.Commands;

public class CreateOrganisationHandlerTests
{
    private readonly AppFactory _factory = new AppFactory();
    private readonly IMediator _mediator;
    private readonly TaskDbContext _dbContext;

    public CreateOrganisationHandlerTests()
    {
        var scope = _factory.Services.CreateScope();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    }

    [Fact]
    public async Task Should_CreateOrganisationInDatabase_WhenCommandIsExecuted()
    {
        // Arrange
        var command = Any.CreateOrganisationCommand();

        // Act
        await _mediator.Send(command);

        // Assert
        Assert.Single(_dbContext.Organisations);
    }

    [Fact]
    public async Task Should_StoreCorrectValues_WhenCommandIsExecuted()
    {
        // Arrange
        var command = Any.CreateOrganisationCommand();

        // Act
        await _mediator.Send(command);

        // Assert
        var organisation = _dbContext.Organisations.Single();
        Assert.Equal(command.Id, organisation.Id);
        Assert.Equal(command.Name, organisation.Name);
        Assert.Equal(command.ExternalId, organisation.ExternalId);
    }
}
