using UniTask.Api.Models;

namespace UniTask.Tests;

/// <summary>
/// Static helper class for generating random test data.
/// This class provides methods to create random primitive values and complex objects
/// with randomized properties, making tests more maintainable and less brittle.
/// </summary>
public static class Any
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Generates a random string of the specified length.
    /// </summary>
    /// <param name="length">Length of the string to generate (default: 10)</param>
    /// <returns>A random string containing lowercase letters</returns>
    public static string String(int length = 10)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[_random.Next(chars.Length)])
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
        return _random.Next(min, max);
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
        return values[_random.Next(values.Length)];
    }

    /// <summary>
    /// Generates a random DateTime.
    /// </summary>
    /// <param name="minDaysAgo">Minimum days in the past (default: 0)</param>
    /// <param name="maxDaysAgo">Maximum days in the past (default: 365)</param>
    /// <returns>A random DateTime</returns>
    public static DateTime DateTime(int minDaysAgo = 0, int maxDaysAgo = 365)
    {
        var daysAgo = _random.Next(minDaysAgo, maxDaysAgo);
        return System.DateTime.UtcNow.AddDays(-daysAgo);
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
            UpdatedAt = updatedAt ?? created.AddDays(_random.Next(0, 30))
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
    /// <param name="sprintId">Optional sprint ID</param>
    /// <returns>A TaskItem with random data</returns>
    public static TaskItem TaskItem(
        string? title = null,
        string? description = null,
        TaskPriority? priority = null,
        int? projectId = null,
        int? taskTypeId = null,
        int? statusId = null,
        int? sprintId = null)
    {
        return new TaskItem
        {
            Title = title ?? $"Task {String(10)}",
            Description = description ?? String(30),
            Priority = priority ?? Enum<TaskPriority>(),
            ProjectId = projectId,
            TaskTypeId = taskTypeId,
            StatusId = statusId,
            SprintId = sprintId,
            CreatedAt = DateTime(),
            UpdatedAt = DateTime(),
            OldStatus = Enum<Api.Models.TaskStatus>()
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
        int? projectId = null)
    {
        return new TaskType
        {
            Name = name ?? $"TaskType {String(6)}",
            Description = description ?? String(20),
            ProjectId = projectId
        };
    }

    /// <summary>
    /// Creates a Sprint with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="goal">Optional goal (randomly generated if not provided)</param>
    /// <param name="projectId">Project ID (default: 1)</param>
    /// <param name="startDate">Optional start date (randomly generated if not provided)</param>
    /// <param name="endDate">Optional end date (randomly generated if not provided)</param>
    /// <returns>A Sprint with random data</returns>
    public static Sprint Sprint(
        string? name = null,
        string? goal = null,
        int projectId = 1,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime(0, 60);
        return new Sprint
        {
            Name = name ?? $"Sprint {String(5)}",
            Goal = goal ?? String(25),
            ProjectId = projectId,
            StartDate = start,
            EndDate = endDate ?? start.AddDays(_random.Next(7, 21))
        };
    }

    /// <summary>
    /// Creates a Status with random values.
    /// </summary>
    /// <param name="name">Optional name (randomly generated if not provided)</param>
    /// <param name="description">Optional description (randomly generated if not provided)</param>
    /// <param name="order">Optional order (randomly generated if not provided)</param>
    /// <param name="projectId">Optional project ID</param>
    /// <returns>A Status with random data</returns>
    public static Status Status(
        string? name = null,
        string? description = null,
        int? order = null,
        int? projectId = null)
    {
        return new Status
        {
            Name = name ?? $"Status {String(6)}",
            Description = description ?? String(15),
            Order = order ?? Int(0, 10),
            ProjectId = projectId
        };
    }
}
