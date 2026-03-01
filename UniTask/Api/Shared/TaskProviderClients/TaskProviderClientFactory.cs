namespace UniTask.Api.Shared.TaskProviderClients;

public interface ITaskProviderClientFactory
{
    ITaskProviderClient GetClient(TaskProvider provider);
}

public class TaskProviderClientFactory : ITaskProviderClientFactory
{
    private readonly IEnumerable<ITaskProviderClient> _clients;

    public TaskProviderClientFactory(IEnumerable<ITaskProviderClient> clients)
    {
        _clients = clients;
    }

    public ITaskProviderClient GetClient(TaskProvider provider)
    {
        return _clients.FirstOrDefault(c => c.Provider == provider)
            ?? _clients.First(c => c.Provider == TaskProvider.Internal);
    }
}
