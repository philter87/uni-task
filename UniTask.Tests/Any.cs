using UniTask.Api.Projects;
using UniTask.Api.Shared;
using UniTask.Api.Tasks;
using UniTask.Api.Users;

namespace UniTask.Tests;

/// <summary>
/// Static helper class for generating random test data.
/// This class provides methods to create random primitive values and complex objects
/// with randomized properties, making tests more maintainable and less brittle.
/// </summary>
public static class Any
{
    /// <summary>
    /// Generates a random string of the specified length.
    /// </summary>
    /// <param name="length">Length of the string to generate (default: 10)</param>
    /// <returns>A random string containing lowercase letters</returns>
    public static string String(int length = 10)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }

    /// <summary>
    /// Generates a random integer.
    /// </summary>
    /// <param name="min">Minimum value (inclusive, default: 1)</param>
    /// <param name="max">Maximum value (exclusive, default: 1000)</param>
    /// <returns>A random integer</returns>
    public static int Int(int min = 1, int max = 1000)
    {
        return Random.Shared.Next(min, max);
    }

    /// <summary>
    /// Generates a random email address.
    /// </summary>
    /// <returns>A random email address</returns>
    public static string Email()
    {
        return $"{String(8)}@{String(6)}.{String(3)}";
    }

    /// <summary>
    /// Randomly selects a value from the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <returns>A random enum value</returns>
    public static T Enum<T>() where T : struct, System.Enum
    {
        var values = System.Enum.GetValues<T>();
        return values[Random.Shared.Next(values.Length)];
    }

    /// <summary>
    /// Generates a random DateTime.
    /// </summary>
    /// <param name="minDaysAgo">Minimum days in the past (default: 0)</param>
    /// <param name="maxDaysAgo">Maximum days in the past (default: 365)</param>
    /// <returns>A random DateTime</returns>
    public static DateTime DateTime(int minDaysAgo = 0, int maxDaysAgo = 365)
    {
        var daysAgo = Random.Shared.Next(minDaysAgo, maxDaysAgo);
        return System.DateTime.UtcNow.AddDays(-daysAgo);
    }

    /// <summary>
    /// Generates a random hex color code.
    /// </summary>
    /// <returns>A random hex color code in the format #RRGGBB</returns>
    public static string Color()
    {
        return $"#{Int(0, 16777215):X6}";
    }

    /// <summary>
    /// Creates a Project with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="createdAt">Optional creation date (randomly generated if not provided)</param>
    /// <param name="updatedAt">Optional update date (randomly generated if not provided)</param>
    /// <returns>A Project with random data</returns>
    public static Project Project(
        string? name = null,
        string? description = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        var created = createdAt ?? DateTime();
        return new Project
        {
            Name = name ?? $"Project {String(8)}",
            Description = description ?? String(20),
            CreatedAt = created,
            UpdatedAt = updatedAt ?? created.AddDays(Random.Shared.Next(0, 30))
        };
    }

    /// <summary>
    /// Creates a TaskItem with random values.
    /// </summary>
    /// <param name="title">Optional title (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="priority">Optional priority (randomly generated if not provided)</param>
    /// <param name="projectId">Optional project ID</param>
    /// <param name="taskTypeId">Optional task type ID</param>
    /// <param name="statusId">Optional status ID</param>
    /// <param name="boardId">Optional board ID</param>
    /// <returns>A TaskItem with random data</returns>
    public static TaskItem TaskItem(
        string? title = null,
        string? description = null,
        double? priority = null,
        Guid? projectId = null,
        Guid? taskTypeId = null,
        Guid? statusId = null,
        Guid? boardId = null)
    {
        return new TaskItem
        {
            Title = title ?? $"Task {String(10)}",
            Description = description ?? String(30),
            Priority = priority ?? Random.Shared.NextDouble() * 10,
            ProjectId = projectId,
            TaskTypeId = taskTypeId,
            StatusId = statusId,
            BoardId = boardId,
            CreatedAt = DateTime(),
            UpdatedAt = DateTime()
        };
    }

    /// <summary>
    /// Creates a TaskType with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="projectId">Optional project ID</param>
    /// <returns>A TaskType with random data</returns>
    public static TaskType TaskType(
        string? name = null,
        string? description = null,
        Guid? projectId = null)
    {
        return new TaskType
        {
            Name = name ?? $"TaskType {String(6)}",
            Description = description ?? String(20),
            ProjectId = projectId
        };
    }

    /// <summary>
    /// Creates a Board with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="goal">Optional goal (randomly generated if not provided)</param>
    /// <param name="projectId">Project ID (default: 1)</param>
    /// <param name="startDate">Optional start date (randomly generated if not provided)</param>
    /// <param name="endDate">Optional end date (randomly generated if not provided)</param>
    /// <returns>A Board with random data</returns>
    public static Board Board(
        string? name = null,
        string? goal = null,
        Guid projectId = default,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime(0, 60);
        return new Board
        {
            Name = name ?? $"Board {String(5)}",
            Goal = goal ?? String(25),
            ProjectId = projectId,
            StartDate = start,
            EndDate = endDate ?? start.AddDays(Random.Shared.Next(7, 21))
        };
    }

    /// <summary>
    /// Creates a Status with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="order">Optional order (randomly generated if not provided)</param>
    /// <param name="taskTypeId">Optional task type ID</param>
    /// <returns>A Status with random data</returns>
    public static Status Status(
        string? name = null,
        string? description = null,
        int? order = null,
        Guid? taskTypeId = null)
    {
        return new Status
        {
            Name = name ?? $"Status {String(6)}",
            Description = description ?? String(15),
            Order = order ?? Int(0, 10),
            TaskTypeId = taskTypeId
        };
    }

    /// <summary>
    /// Creates a Label with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="typeId">Optional type ID</param>
    /// <returns>A Label with random data</returns>
    public static Label Label(
        string? name = null,
        Guid? typeId = null)
    {
        return new Label
        {
            Name = name ?? $"Label {String(6)}",
            TypeId = typeId
        };
    }

    /// <summary>
    /// Creates a LabelType with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="color">Optional color (randomly generated if not provided)</param>
    /// <returns>A LabelType with random data</returns>
    public static LabelType LabelType(
        string? name = null,
        string? color = null)
    {
        return new LabelType
        {
            Name = name ?? $"LabelType {String(6)}",
            Color = color ?? Color()
        };
    }

    /// <summary>
    /// Creates a Tag with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <returns>A Tag with random data</returns>
    public static Tag Tag(
        string? name = null)
    {
        return new Tag
        {
            Name = name ?? $"Tag {String(6)}"
        };
    }

    /// <summary>
    /// Creates an Attachment with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="url">Optional URL (randomly generated if not provided)</param>
    /// <param name="internalName">Optional internal name (randomly generated if not provided)</param>
    /// <param name="fileType">Optional file type (randomly generated if not provided)</param>
    /// <param name="externalId">Optional external ID</param>
    /// <param name="taskItemId">Task item ID (default: 1)</param>
    /// <returns>An Attachment with random data</returns>
    public static Attachment Attachment(
        string? name = null,
        string? url = null,
        string? internalName = null,
        string? fileType = null,
        string? externalId = null,
        Guid taskItemId = default)
    {
        return new Attachment
        {
            Name = name ?? $"File {String(8)}.pdf",
            Url = url ?? $"https://example.com/files/{String(16)}",
            InternalName = internalName ?? $"{String(32)}.pdf",
            FileType = fileType ?? "application/pdf",
            ExternalId = externalId,
            TaskItemId = taskItemId
        };
    }

    // DTO Factory Methods

    /// <summary>
    /// Creates a ProjectDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="createdAt">Optional creation date (randomly generated if not provided)</param>
    /// <param name="updatedAt">Optional update date (randomly generated if not provided)</param>
    /// <returns>A ProjectDto with random data</returns>
    public static ProjectDto ProjectDto(
        Guid id = default,
        string? name = null,
        string? description = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        var created = createdAt ?? DateTime();
        return new ProjectDto
        {
            Id = id,
            Name = name ?? $"Project {String(8)}",
            Description = description ?? String(20),
            CreatedAt = created,
            UpdatedAt = updatedAt ?? created.AddDays(Random.Shared.Next(0, 30))
        };
    }

    /// <summary>
    /// Creates a StatusDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="order">Optional order (randomly generated if not provided)</param>
    /// <param name="taskTypeId">Optional task type ID</param>
    /// <returns>A StatusDto with random data</returns>
    public static StatusDto StatusDto(
        Guid id = default,
        string? name = null,
        string? description = null,
        int? order = null,
        Guid? taskTypeId = null)
    {
        return new StatusDto
        {
            Id = id,
            Name = name ?? $"Status {String(6)}",
            Description = description ?? String(15),
            Order = order ?? Int(0, 10),
            TaskTypeId = taskTypeId
        };
    }

    /// <summary>
    /// Creates a TaskTypeDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="projectId">Optional project ID</param>
    /// <returns>A TaskTypeDto with random data</returns>
    public static TaskTypeDto TaskTypeDto(
        Guid id = default,
        string? name = null,
        string? description = null,
        Guid? projectId = null)
    {
        return new TaskTypeDto
        {
            Id = id,
            Name = name ?? $"TaskType {String(6)}",
            Description = description ?? String(20),
            ProjectId = projectId
        };
    }

    /// <summary>
    /// Creates a BoardDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="goal">Optional goal (randomly generated if not provided)</param>
    /// <param name="projectId">Project ID (default: 1)</param>
    /// <param name="startDate">Optional start date (randomly generated if not provided)</param>
    /// <param name="endDate">Optional end date (randomly generated if not provided)</param>
    /// <returns>A BoardDto with random data</returns>
    public static BoardDto BoardDto(
        Guid id = default,
        string? name = null,
        string? goal = null,
        Guid projectId = default,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime(0, 60);
        return new BoardDto
        {
            Id = id,
            Name = name ?? $"Board {String(5)}",
            Goal = goal ?? String(25),
            ProjectId = projectId,
            StartDate = start,
            EndDate = endDate ?? start.AddDays(Random.Shared.Next(7, 21))
        };
    }

    /// <summary>
    /// Creates a CommentDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="taskItemId">Task item ID (default: 1)</param>
    /// <param name="content">Optional content (randomly generated if not provided)</param>
    /// <param name="userId">Optional user ID (randomly generated if not provided)</param>
    /// <param name="createdAt">Optional creation date (randomly generated if not provided)</param>
    /// <param name="updatedAt">Optional update date</param>
    /// <returns>A CommentDto with random data</returns>
    public static CommentDto CommentDto(
        Guid id = default,
        Guid taskItemId = default,
        string? content = null,
        Guid? userId = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new CommentDto
        {
            Id = id,
            TaskItemId = taskItemId,
            Content = content ?? String(50),
            UserId = userId,
            CreatedAt = createdAt ?? DateTime(),
            UpdatedAt = updatedAt
        };
    }

    /// <summary>
    /// Creates a LabelDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="typeId">Optional type ID</param>
    /// <returns>A LabelDto with random data</returns>
    public static LabelDto LabelDto(
        Guid id = default,
        string? name = null,
        Guid? typeId = null)
    {
        return new LabelDto
        {
            Id = id,
            Name = name ?? $"Label {String(6)}",
            TypeId = typeId
        };
    }

    /// <summary>
    /// Creates a TagDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <returns>A TagDto with random data</returns>
    public static TagDto TagDto(
        Guid id = default,
        string? name = null)
    {
        return new TagDto
        {
            Id = id,
            Name = name ?? $"Tag {String(6)}"
        };
    }

    /// <summary>
    /// Creates an AttachmentDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="url">Optional URL (randomly generated if not provided)</param>
    /// <param name="internalName">Optional internal name (randomly generated if not provided)</param>
    /// <param name="fileType">Optional file type (randomly generated if not provided)</param>
    /// <param name="externalId">Optional external ID</param>
    /// <returns>An AttachmentDto with random data</returns>
    public static AttachmentDto AttachmentDto(
        Guid id = default,
        string? name = null,
        string? url = null,
        string? internalName = null,
        string? fileType = null,
        string? externalId = null)
    {
        return new AttachmentDto
        {
            Id = id,
            Name = name ?? $"File {String(8)}.pdf",
            Url = url ?? $"https://example.com/files/{String(16)}",
            InternalName = internalName ?? $"{String(32)}.pdf",
            FileType = fileType ?? "application/pdf",
            ExternalId = externalId
        };
    }

    /// <summary>
    /// Creates a UniUser with random values.
    /// </summary>
    /// <param name="email">Optional email (randomly generated if not provided)</param>
    /// <param name="username">Optional username (randomly generated if not provided)</param>
    /// <param name="displayName">Optional display name (randomly generated if not provided)</param>
    /// <param name="externalId">Optional external ID</param>
    /// <returns>A UniUser with random data</returns>
    public static UniUser User(
        string? email = null,
        string? username = null,
        string? displayName = null,
        string? externalId = null)
    {
        return new UniUser
        {
            Email = email ?? Email(),
            UserName = username ?? $"user_{String(8)}",
            DisplayName = displayName ?? String(12),
            ExternalId = externalId
        };
    }

    /// <summary>
    /// Creates an Organisation with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="externalId">Optional external ID</param>
    /// <returns>An Organisation with random data</returns>
    public static Organisation Organisation(
        string? name = null,
        string? externalId = null)
    {
        return new Organisation
        {
            Name = name ?? $"Org {String(8)}",
            ExternalId = externalId
        };
    }

    /// <summary>
    /// Creates a TaskProviderAuth with random values.
    /// </summary>
    /// <param name="authenticationType">Optional authentication type (randomly selected if not provided)</param>
    /// <param name="authTypeId">Optional auth type ID / client ID (randomly generated if not provided)</param>
    /// <param name="secretValue">Optional secret value (randomly generated if not provided)</param>
    /// <param name="organisationId">Organisation ID (default: 0, must be set before saving)</param>
    /// <returns>A TaskProviderAuth with random data</returns>
    public static TaskProviderAuth TaskProviderAuth(
        AuthenticationType? authenticationType = null,
        string? authTypeId = null,
        string? secretValue = null,
        Guid organisationId = default)
    {
        return new TaskProviderAuth
        {
            AuthenticationType = authenticationType ?? Enum<AuthenticationType>(),
            AuthTypeId = authTypeId ?? String(20),
            SecretValue = secretValue ?? String(40),
            OrganisationId = organisationId
        };
    }

    /// <summary>
    /// Creates a TaskItemDto with random values.
    /// </summary>
    /// <param name="id">Optional ID (default: 0)</param>
    /// <param name="title">Optional title (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="priority">Optional priority (randomly generated if not provided)</param>
    /// <param name="projectId">Optional project ID</param>
    /// <param name="taskTypeId">Optional task type ID</param>
    /// <param name="statusId">Optional status ID</param>
    /// <param name="boardId">Optional board ID</param>
    /// <param name="createdAt">Optional creation date (randomly generated if not provided)</param>
    /// <param name="updatedAt">Optional update date (randomly generated if not provided)</param>
    /// <param name="dueDate">Optional due date</param>
    /// <param name="assignedTo">Optional assignee</param>
    /// <param name="provider">Optional provider</param>
    /// <param name="externalId">Optional external ID</param>
    /// <param name="durationHours">Optional duration in hours</param>
    /// <param name="durationRemainingHours">Optional remaining time in hours</param>
    /// <returns>A TaskItemDto with random data</returns>
    public static TaskItemDto TaskItemDto(
        Guid id = default,
        string? title = null,
        string? description = null,
        double? priority = null,
        Guid? projectId = null,
        Guid? taskTypeId = null,
        Guid? statusId = null,
        Guid? boardId = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? dueDate = null,
        string? assignedTo = null,
        Guid? assignedToUserId = null,
        TaskProvider? provider = null,
        string? externalId = null,
        double? durationHours = null,
        double? durationRemainingHours = null)
    {
        var created = createdAt ?? DateTime();
        return new TaskItemDto
        {
            Id = id,
            Title = title ?? $"Task {String(10)}",
            Description = description ?? String(30),
            Priority = priority ?? Random.Shared.NextDouble() * 10,
            ProjectId = projectId,
            TaskTypeId = taskTypeId,
            StatusId = statusId,
            BoardId = boardId,
            CreatedAt = created,
            UpdatedAt = updatedAt ?? created,
            DueDate = dueDate,
            AssignedTo = assignedTo,
            AssignedToUserId = assignedToUserId,
            Provider = provider,
            ExternalId = externalId,
            DurationHours = durationHours,
            DurationRemainingHours = durationRemainingHours
        };
    }
}
