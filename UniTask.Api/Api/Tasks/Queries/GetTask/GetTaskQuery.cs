using MediatR;

namespace UniTask.Api.Tasks.Queries.GetTask;

public class GetTaskQuery : IRequest<TaskItemDto?>
{
    public int Id { get; set; }
}
