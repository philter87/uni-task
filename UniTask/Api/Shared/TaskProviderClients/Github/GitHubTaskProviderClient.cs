using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Projects;
using UniTask.Api.Projects.Create;
using UniTask.Api.Projects.GetProjects;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Create;
using UniTask.Api.Tasks.GetTasks;

namespace UniTask.Api.Shared.TaskProviderClients;

public class GitHubTaskProviderClient : ITaskProviderClient
{
    private readonly IGitHubHttpClientFactory _gitHubClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;

    public TaskProvider Provider => TaskProvider.GitHub;

    public GitHubTaskProviderClient(IGitHubHttpClientFactory gitHubClientFactory, IServiceScopeFactory scopeFactory)
    {
        _gitHubClientFactory = gitHubClientFactory;
        _scopeFactory = scopeFactory;
    }

    public Task CreateProject(ProjectCreatedEvent projectCreated) => Task.CompletedTask;

    public Task<IEnumerable<ProjectDto>> GetProjects(GetProjectsQuery getProjects) =>
        Task.FromResult(Enumerable.Empty<ProjectDto>());

    public Task CreateTask(TaskCreatedEvent taskCreated) => Task.CompletedTask;

    public async Task<IEnumerable<TaskItemDto>> GetTasks(GetTasksQuery getTasks)
    {
        if (string.IsNullOrEmpty(getTasks.ExternalProjectId))
            return Enumerable.Empty<TaskItemDto>();

        if (getTasks.ProjectId.HasValue && _gitHubClientFactory.IsConfiguredForProject(getTasks.ProjectId.Value))
        {
            var httpClient = _gitHubClientFactory.CreateClientForProject(getTasks.ProjectId.Value);
            return await FetchIssuesAsync(httpClient, getTasks);
        }

        var organisationId = await GetOrganisationIdAsync(getTasks.ProjectId);

        if (!_gitHubClientFactory.IsConfigured(organisationId))
            return Enumerable.Empty<TaskItemDto>();

        var client = _gitHubClientFactory.CreateClient(organisationId);
        return await FetchIssuesAsync(client, getTasks);
    }

    private async Task<IEnumerable<TaskItemDto>> FetchIssuesAsync(HttpClient httpClient, GetTasksQuery getTasks)
    {
        List<GitHubIssue>? issues;
        try
        {
            issues = await httpClient.GetFromJsonAsync<List<GitHubIssue>>(
                $"/repos/{getTasks.ExternalProjectId}/issues?state=all&per_page=100");
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

    private async Task<Guid> GetOrganisationIdAsync(Guid? projectId)
    {
        if (projectId == null)
            return Guid.Empty;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        var project = await context.Projects.FindAsync(projectId);
        return project?.OrganisationId ?? Guid.Empty;
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
