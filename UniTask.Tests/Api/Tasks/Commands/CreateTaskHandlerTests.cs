using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Create;
using UniTask.Tests.Utls;
using Xunit;

namespace UniTask.Tests.Api.Tasks.Commands;

public class CreateTaskHandlerTests
{
    private readonly AppFactory _factory = new AppFactory();
    private readonly IMediator _mediator;
    private readonly TaskDbContext _dbContext;

    public CreateTaskHandlerTests()
    {
        var scope = _factory.Services.CreateScope();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    }

    [Fact]
    public async Task Should_CreateTaskInDatabase_WhenCommandIsExecuted()
    {
        // Arrange
        var command = Any.CreateTaskCommand();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.Single(_dbContext.Tasks);
        Assert.IsType<TaskCreatedEvent>(result);
        Assert.Equal(command.Title, result.Title);
    }

    [Fact]
    public async Task Should_ReturnTaskCreatedEvent_WithCorrectData()
    {
        // Arrange
        var command = Any.CreateTaskCommand(title: "My Task");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.Equal("My Task", result.Title);
        Assert.NotEqual(default, result.TaskId);
        Assert.NotEqual(default, result.CreatedAt);
    }
}
