# GitHub HTTP Client Factory

## Overview

The `GitHubHttpClientFactory` provides a reusable, properly configured HTTP client for interacting with the GitHub REST API. It handles authentication, headers, and base URL configuration automatically.

## Features

- ✅ Automatic authentication with GitHub token
- ✅ Proper GitHub API headers (Accept, User-Agent, API version)
- ✅ Base URL configuration
- ✅ Configuration validation
- ✅ Centralized logging
- ✅ Easy to test and mock

## Configuration

Add your GitHub credentials to `appsettings.json`:

```json
{
  "GitHub": {
    "Token": "your_github_token_here",
    "Owner": "your_github_username_or_org",
    "Repo": "your_repository_name"
  }
}
```

## Usage

### 1. Inject the Factory

```csharp
public class MyEventHandler : INotificationHandler<SomeEvent>
{
    private readonly IGitHubHttpClientFactory _gitHubClientFactory;
    
    public MyEventHandler(IGitHubHttpClientFactory gitHubClientFactory)
    {
        _gitHubClientFactory = gitHubClientFactory;
    }
}
```

### 2. Check Configuration

```csharp
if (!_gitHubClientFactory.IsConfigured())
{
    _logger.LogWarning("GitHub is not configured. Skipping...");
    return;
}
```

### 3. Create HTTP Client

```csharp
var httpClient = _gitHubClientFactory.CreateClient();
var owner = _gitHubClientFactory.GetOwner();
var repo = _gitHubClientFactory.GetRepo();
```

### 4. Make API Calls

```csharp
// Example: Create an issue
var body = new
{
    title = "My Issue",
    body = "Issue description"
};

var request = new HttpRequestMessage(HttpMethod.Post, $"/repos/{owner}/{repo}/issues")
{
    Content = JsonContent.Create(body)
};

var response = await httpClient.SendAsync(request, cancellationToken);
```

## Complete Example

Here's a complete example of creating a GitHub issue from a `TaskCreatedEventHandler`:

```csharp
public async Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
{
    // Check if GitHub provider
    if (notification.TaskProvider != TaskProvider.GitHub)
        return;

    // Validate configuration
    if (!_gitHubClientFactory.IsConfigured())
    {
        _logger.LogWarning("GitHub configuration is missing. Skipping GitHub issue creation.");
        return;
    }

    // Get configured client and settings
    var httpClient = _gitHubClientFactory.CreateClient();
    var owner = _gitHubClientFactory.GetOwner();
    var repo = _gitHubClientFactory.GetRepo();

    // Prepare request
    var body = new
    {
        title = notification.Title,
        body = notification.Description
    };

    var request = new HttpRequestMessage(HttpMethod.Post, $"/repos/{owner}/{repo}/issues")
    {
        Content = JsonContent.Create(body)
    };

    // Send request
    var response = await httpClient.SendAsync(request, cancellationToken);

    // Handle response
    if (!response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Failed to create GitHub issue. Status: {StatusCode}. Response: {Content}",
            response.StatusCode, content);
    }
    else
    {
        _logger.LogInformation("Successfully created GitHub issue");
    }
}
```

> **Note:** The `ProjectCreatedEvent` is intentionally **not** synced to GitHub. A UniTask project maps to a GitHub *repository*, not a GitHub *project*. GitHub projects are a separate planning concept (kanban boards/sprints) unrelated to UniTask projects. When using GitHub as a task provider, configure the target repository in `appsettings.json` instead.

## Benefits

1. **Reusability**: Use the same factory across all GitHub integrations (Tasks, Projects, Pull Requests, etc.)
2. **Consistency**: All GitHub API calls use the same headers and authentication
3. **Testability**: Easy to mock `IGitHubHttpClientFactory` in unit tests
4. **Maintainability**: Configuration changes in one place affect all usages
5. **Security**: Token management is centralized and not scattered across the codebase

## API Methods

### `CreateClient()`
Creates a configured HttpClient with:
- Base URL: `https://api.github.com`
- Authorization header with Bearer token
- Accept header: `application/vnd.github+json`
- User-Agent header: `UniTask/1.0`
- X-GitHub-Api-Version header: `2022-11-28`

### `GetOwner()`
Returns the configured GitHub owner (username or organization).

### `GetRepo()`
Returns the configured GitHub repository name.

### `IsConfigured()`
Returns `true` if Token, Owner, and Repo are all configured, `false` otherwise.

## Testing

Mock the interface in your tests:

```csharp
var mockFactory = new Mock<IGitHubHttpClientFactory>();
mockFactory.Setup(f => f.IsConfigured()).Returns(true);
mockFactory.Setup(f => f.GetOwner()).Returns("test-owner");
mockFactory.Setup(f => f.GetRepo()).Returns("test-repo");
mockFactory.Setup(f => f.CreateClient()).Returns(mockHttpClient);
```

