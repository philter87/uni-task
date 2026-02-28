# UniTask Development Guide

## Architecture Overview

UniTask uses **CQRS with MediatR** and **Domain-Driven Design (DDD)** to create a unified task management API. Commands modify state and trigger domain events stored on entities. External provider integrations (GitHub, Azure DevOps, etc.) belong in **EventHandlers**.

### CQRS + DDD Flow
```
Controller → Command → Handler → Entity Factory (adds DomainEvents) → Publish DomainEvents → SaveChanges
                                                                             ↓
                                                                        EventHandler (external integrations)
```

**Key Rules:**
- **Commands**: Imperative verbs (`CreateProject`, `ChangeTaskStatus`) - modify state via entity methods
- **Queries**: Start with `Get` (`GetAllProjects`, `GetTaskById`) - read-only, query `TaskDbContext` directly
- **Entities**: Have static factory methods (e.g., `Project.Create()`) that add domain events to `DomainEvents` collection
- **Domain Events**: Past tense (`ProjectCreatedEvent`, `TaskStatusChangedEvent`) - stored on entities, published before `SaveChanges`
- **Handlers**: Inject `TaskDbContext` and `IPublisher`, call entity factories, use `_publisher.PublishAll(entity.DomainEvents)` before `SaveChangesAsync`
- **EventHandlers**: Implement `INotificationHandler<TEvent>` for side effects and external integrations

**Example:** See [CreateProjectCommandHandler](UniTask.Api/Api/Projects/Commands/Create/CreateProjectCommandHandler.cs) → [Project.Create()](UniTask.Api/Api/Projects/Models/Project.cs)

## Project Structure (Feature-Based)

```
UniTask.Api/Api/
├── Projects/                    # Project feature
│   ├── Commands/Create/         # CreateProject CQRS components (commands only)
│   │   ├── CreateProjectCommand.cs
│   │   └── CreateProjectCommandHandler.cs
│   ├── Queries/GetProject/      # Queries organized similarly
│   ├── Models/                  # DB entity models
│   │   ├── Project.cs
│   │   ├── Organisation.cs
│   │   ├── Board.cs
│   │   └── ...
│   ├── Events/                  # Events and EventHandlers
│   │   ├── ProjectCreatedEvent.cs
│   │   └── ProjectCreatedEventHandler.cs
│   ├── ProjectDto.cs            # API contract
│   └── ProjectsController.cs
├── Tasks/                       # Task feature (same structure)
│   ├── Commands/
│   │   ├── Create/, Update/, Delete/
│   │   ├── ChangeStatus/, AddLabel/, RemoveLabel/
│   │   └── AssignMember/
│   ├── Queries/
│   │   ├── GetTask/, GetTasks/
│   ├── Models/                  # DB entity models
│   │   ├── TaskItem.cs
│   │   ├── Comment.cs
│   │   └── ...
│   ├── Events/                  # Events and EventHandlers
│   │   ├── TaskCreatedEvent.cs
│   │   ├── TaskCreatedEventHandler.cs
│   │   └── ...
│   └── TaskItemMapper.cs        # Shared entity → DTO mapping
└── Shared/                      # Cross-feature components
    ├── TaskDbContext.cs
    ├── TaskProvider.cs          # TaskProvider enum (Internal, GitHub, AzureDevOps, Jira)
    └── [Status, TaskType, Label, Comment models/DTOs]
```

**Navigation tip:** All code for a feature lives in its folder. New commands/queries go in their own subfolders. Events go in the feature-level `Events/` folder.

## Adding New CQRS Operations

### For Commands (State Changes)

1. **Create Command** in `Commands/{Operation}/`
   - Implement `IRequest` or `IRequest<TResponse>`
   - Implement `IProviderEvent` (includes `Origin` and `TaskProvider`)
   - Example: [CreateProjectCommand.cs](UniTask.Api/Api/Projects/Commands/Create/CreateProjectCommand.cs)

2. **Create Event** in `Events/`
   - Implement `INotification` and `IProviderEvent`
   - Use past tense naming (e.g., `ProjectCreatedEvent`)
   - Example: [ProjectCreatedEvent.cs](UniTask.Api/Api/Projects/Events/ProjectCreatedEvent.cs)

3. **Add Domain Event to Entity**
   - Entities have `[NotMapped] public List<INotification> DomainEvents { get; private set; } = new();`
   - Create static factory method (e.g., `Project.Create(command)`) that:
     - Creates the entity
     - Adds domain event to `DomainEvents` collection
     - Returns the entity
   - Example: [Project.Create()](UniTask.Api/Api/Projects/Models/Project.cs)

4. **Create Handler** in `Commands/{Operation}/`
   - Implement `IRequestHandler<TCommand>` or `IRequestHandler<TCommand, TResponse>`
   - Inject `TaskDbContext` and `IPublisher`
   - Call entity factory method
   - Add entity to `DbContext`
   - Call `await _publisher.PublishAll(entity.DomainEvents, cancellationToken)` BEFORE `SaveChangesAsync`
   - Call `await _context.SaveChangesAsync(cancellationToken)`
   - Example: [CreateProjectCommandHandler.cs](UniTask.Api/Api/Projects/Commands/Create/CreateProjectCommandHandler.cs)

5. **Create EventHandler** (optional, for side effects)
   - Implement `INotificationHandler<TEvent>` in `Events/`
   - Handle external integrations, notifications, etc.

6. **Add Controller endpoint** using `await _mediator.Send(command)`

### For Queries (Read-Only)

1. **Create Query** in `Queries/{Operation}/`
   - Implement `IRequest<TResponse>`
2. **Create Handler** implementing `IRequestHandler<TQuery, TResponse>`
   - Inject `TaskDbContext`
   - Query database and return result
3. **Add Controller endpoint** using `await _mediator.Send(query)`

## External Integrations (GitHub, Azure DevOps)

External provider logic belongs in **EventHandlers**, not in command/query handlers. This keeps the core flow clean and makes integrations opt-in side effects.

**Example:** Syncing a created task to GitHub Issues:
- `CreateTaskCommandHandler` saves to DB and publishes `TaskCreatedEvent`
- A `GitHubTaskSyncEventHandler` (implementing `INotificationHandler<TaskCreatedEvent>`) calls the GitHub API

Refer to `.github/skills/github-issues-rest-api/` for GitHub API patterns.

## Testing Conventions

### Integration Testing Approach

**Preferred**: Use `CustomWebApplicationFactory` with full DI container and in-memory database. This tests the complete CQRS flow.

**Pattern:**
```csharp
public class CreateProjectHandlerTests
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly IMediator _mediator;
    private readonly TaskDbContext _dbContext;

    public CreateProjectHandlerTests()
    {
        var scope = _factory.Services.CreateScope();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    }
    
    [Fact]
    public async Task Should_CreateProjectInDatabase_WhenCommandIsExecuted()
    {
        // Arrange
        var command = Any.CreateProjectCommand();

        // Act
        await _mediator.Send(command);
        
        // Assert
        Assert.Single(_dbContext.Projects);
    }
}
```

**Key Points:**
- Get `IMediator` and `TaskDbContext` from DI container
- Send commands through MediatR (tests full pipeline)
- Assert on database state
- See [CreateProjectHandlerTests.cs](UniTask.Tests/Api/Projects/Commands/CreateProjectHandlerTests.cs)

### Test Data Generation with `Any`

Use the **`Any` class** ([Any.cs](UniTask.Tests/Utls/Any.cs)) for all test data:

```csharp
// Commands
var command = Any.CreateProjectCommand();
var commandWithOverrides = Any.CreateProjectCommand(name: "Specific Name");

// Entities
var project = Any.Project();
var task = Any.TaskItem(title: "Specific Title");

// Primitive values
var randomString = Any.String(10);
var randomInt = Any.Int(1, 100);
var randomEmail = Any.Email();
var randomEnum = Any.Enum<TaskProvider>();
```

**Why `Any`?**
- Focuses tests on behavior, not data
- Prevents coupling to specific test values
- Reduces boilerplate
- Override only what matters for the test

**When adding new commands**: Add a corresponding factory method to `Any` class.

## Technology Stack

- **Backend**: ASP.NET Core (.NET 10), MediatR, EF Core with SQLite
- **Frontend**: React 19, Vite, TypeScript, TailwindCSS, Radix UI
- **API Docs**: NSwag/OpenAPI (available at `/swagger` in dev mode)
- **Auth**: ASP.NET Core Identity (configured but not fully implemented)

## Development Workflows

**Run backend + frontend:**
```bash
cd UniTask.Api
dotnet run  # API on https://localhost:5001, serves React build at /
```

**Frontend development (hot reload):**
```bash
cd UniTask.Api/ClientApp
npm run dev  # Vite dev server on http://localhost:5173
```

**Run tests:**
```bash
cd UniTask.Tests
dotnet test
```

**Database:**
- SQLite (`tasks.db`) auto-created in dev mode via `db.Database.EnsureCreated()`
- Connection string in `appsettings.json`
- No migrations yet - using EnsureCreated for simplicity

## Skills System

**`.github/skills/`** contains domain knowledge for AI agents. Each skill has:
- **`SKILL.md`**: Main documentation with YAML frontmatter
- **`examples/`**: JSON examples with detailed usage patterns

**Example:** [`github-issues-rest-api`](.github/skills/github-issues-rest-api/SKILL.md) skill documents GitHub REST API for future GitHub EventHandler implementations.

**When to create a skill:** External system integration, complex domain logic, or API interaction patterns.

## Key Conventions

- **DTOs end with `Dto`**: `ProjectDto`, `TaskItemDto` (API contracts)
- **Entity models**: Plain classes without suffix (`Project`, `TaskItem`)
- **Events end with `Event`**: `ProjectCreatedEvent`, `TaskStatusChangedEvent`
- **Handlers end with `Handler`**: `CreateProjectCommandHandler`, `GetProjectQueryHandler`

## Common Pitfalls

1. **Don't bypass MediatR** - Controllers should only call `_mediator.Send()`, never `TaskDbContext` directly
2. **Publish events BEFORE SaveChanges** - Always call `_publisher.PublishAll(entity.DomainEvents)` before `SaveChangesAsync` for transactional consistency
3. **Domain events live on entities** - Don't publish events directly from handlers; add them to entity's `DomainEvents` collection in factory methods
4. **Use entity factory methods** - Don't construct entities with `new` in handlers; use static factory methods like `Project.Create(command)`
5. **Events are for side effects** - Main operation result goes through command response; events handle notifications, logging, and external integrations
6. **External integrations go in EventHandlers** - Don't call GitHub/Azure DevOps APIs from command handlers
7. **Each CQRS operation gets its own folder** - Don't cram multiple commands in one file/folder
8. **Add corresponding `Any` methods** - When creating new commands, add factory methods to `Any` class for testing

## Frontend Integration

- **API Client**: Auto-generated TypeScript client from NSwag (see [`ClientApp/src/api/`](UniTask.Api/ClientApp/src/api/))
- **Components**: Reusable UI in [`ClientApp/src/components/ui/`](UniTask.Api/ClientApp/src/components/ui/) using Radix UI primitives
- **Styling**: TailwindCSS with custom config in [`tailwind.config.js`](UniTask.Api/ClientApp/tailwind.config.js)

## Future Backend Integrations

To add GitHub/Azure DevOps support:
1. Create an EventHandler implementing `INotificationHandler<TEvent>` for the relevant event(s)
2. Call the external API inside the handler
3. Refer to `.github/skills/github-issues-rest-api/` for GitHub API patterns

