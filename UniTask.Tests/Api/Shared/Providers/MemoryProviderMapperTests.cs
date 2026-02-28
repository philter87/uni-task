using UniTask.Api.Projects.Events;
using UniTask.Api.Shared.TaskProviderClients;
using UniTask.Api.Tasks.Events;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Shared.Providers;

public class MemoryProviderMapperTests
{
    [Fact]
    public void MapToProjectDto_MapsAllFields()
    {
        var evt = new ProjectCreatedEvent
        {
            ProjectId = Guid.NewGuid(),
            Name = Any.String(),
            Description = Any.String(),
            CreatedAt = Any.DateTime()
        };

        var dto = MemoryProviderMapper.MapToProjectDto(evt);

        Assert.Equal(evt.ProjectId, dto.Id);
        Assert.Equal(evt.Name, dto.Name);
        Assert.Equal(evt.Description, dto.Description);
        Assert.Equal(evt.CreatedAt, dto.CreatedAt);
        Assert.Equal(evt.CreatedAt, dto.UpdatedAt);
    }

    [Fact]
    public void MapToTaskItemDto_MapsAllFields()
    {
        var evt = new TaskCreatedEvent
        {
            TaskId = Guid.NewGuid(),
            Title = Any.String(),
            CreatedAt = Any.DateTime()
        };

        var dto = MemoryProviderMapper.MapToTaskItemDto(evt);

        Assert.Equal(evt.TaskId, dto.Id);
        Assert.Equal(evt.Title, dto.Title);
        Assert.Equal(evt.CreatedAt, dto.CreatedAt);
        Assert.Equal(evt.CreatedAt, dto.UpdatedAt);
    }
}
