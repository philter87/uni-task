using System.Net.Http.Json;
using System.Text.Json.Serialization;
using UniTask.Api.Projects;
using UniTask.Api.Projects.Events;
using UniTask.Api.Projects.Queries.GetProjects;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Events;
using UniTask.Api.Tasks.Queries.GetTasks;

namespace UniTask.Api.Shared.TaskProviderClients;

public class GitHubTaskProviderClient : ITaskProviderClient
{
    private const string GitHubApiBaseUrl = "https://api.github.com";

    private readonly HttpClient _httpClient;

    public GitHubTaskProviderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task CreateProject(ProjectCreatedEvent projectCreated) => Task.CompletedTask;

    public Task<IEnumerable<ProjectDto>> GetProjects(GetProjectsQuery getProjects) =>
        Task.FromResult(Enumerable.Empty<ProjectDto>());

    public Task CreateTask(TaskCreatedEvent taskCreated) => Task.CompletedTask;

    public async Task<IEnumerable<TaskItemDto>> GetTasks(GetTasksQuery getTasks)
    {
        if (string.IsNullOrEmpty(getTasks.ExternalProjectId))
            return Enumerable.Empty<TaskItemDto>();

        List<GitHubIssue>? issues;
        try
        {
            issues = await _httpClient.GetFromJsonAsync<List<GitHubIssue>>(
                $"{GitHubApiBaseUrl}/repos/{getTasks.ExternalProjectId}/issues?state=all&per_page=100");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Failed to fetch issues from GitHub repository '{getTasks.ExternalProjectId}': {ex.Message}", ex);
        }

        return issues?.Select(issue => new TaskItemDto
        {
            Id = default,
            ExternalId = issue.Number.ToString(),
            Title = issue.Title,
            Description = issue.Body,
            ProjectId = getTasks.ProjectId,
            Provider = TaskProvider.GitHub,
            AssignedTo = issue.Assignee?.Login,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt
        }) ?? Enumerable.Empty<TaskItemDto>();
    }

    private class GitHubIssue
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("assignee")]
        public GitHubUser? Assignee { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    private class GitHubUser
    {
        [JsonPropertyName("login")]
        public string Login { get; set; } = string.Empty;
    }
}
