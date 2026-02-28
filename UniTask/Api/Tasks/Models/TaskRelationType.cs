namespace UniTask.Api.Tasks.Models;

public enum TaskRelationType
{
    Undefined,
    DependsOn,
    Mentions,
    IsMentionedBy,
    Parent,
    Child,
    Blocks,
    IsBlockedBy,
    Duplicates,
    IsDuplicatedBy,
    RelatesTo
}
