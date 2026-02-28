using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Shared;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Projects.Commands;

public class CreateProjectHandlerTests
{
    private readonly CustomWebApplicationFactory _factory = new CustomWebApplicationFactory();
    private readonly IMediator _mediator;
    private readonly TaskDbContext _dbContext;

    public CreateProjectHandlerTests()
    {
        var scope = _factory.Services.CreateScope();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    }
    
    [Fact]
    public async Task Should_CreateProjectInDatabase_WhenCommandIsExecuted()
    {
        // Arrange
        var createProjectCommand = Any.CreateProjectCommand();

        // Act
        await _mediator.Send(createProjectCommand);
        
        // Assert
        Assert.Single(_dbContext.Projects);
    }
}