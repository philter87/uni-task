namespace UniTask.Api.Shared;

public interface IProviderEvent
{
    ChangeOrigin Origin { get; set; }
    TaskProvider TaskProvider { get; set; }
}
