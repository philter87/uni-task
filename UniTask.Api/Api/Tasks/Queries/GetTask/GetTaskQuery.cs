using MediatR;

namespace UniTask.Api.Tasks.Queries.GetTask;

public class GetTaskQuery : IRequest<TaskItemDto?>
{
    public Guid Id { get; set; }
}
