# UniTask Development Guide

## Architecture Overview

UniTask uses **CQRS with MediatR** to create a unified task management API. Commands and queries interact directly with the database via `TaskDbContext`. External provider integrations (GitHub, Azure DevOps, etc.) belong in **EventHandlers**.

### CQRS Flow
```
Controller → Command/Query → Handler → DbContext → Event (for commands)
                                                        ↓
                                                   EventHandler (external integrations)
```

**Key Rules:**
- **Commands**: Imperative verbs (`CreateProject`, `ChangeTaskStatus`) - modify state, interact with `TaskDbContext` directly
- **Queries**: Start with `Get` (`GetAllProjects`, `GetTaskById`) - read-only, query `TaskDbContext` directly
- **Events**: Past tense (`ProjectCreated`, `TaskStatusChanged`) - published by CommandHandlers, handled by EventHandlers for side effects and external integrations

**Example:** See [`ProjectsController`](UniTask.Api/Api/Projects/ProjectsController.cs) → [`CreateProjectCommandHandler`](UniTask.Api/Api/Projects/Commands/Create/CreateProjectCommandHandler.cs)

## Project Structure (Feature-Based)

```
UniTask.Api/Api/
├── Projects/                    # Project feature
│   ├── Commands/Create/         # CreateProject CQRS components
│   │   ├── CreateProjectCommand.cs
│   │   ├── CreateProjectCommandHandler.cs
│   │   ├── ProjectCreatedEvent.cs
│   │   └── ProjectCreatedEventHandler.cs
│   ├── Queries/GetProject/      # Queries organized similarly
│   ├── Project.cs               # Entity model
│   ├── ProjectDto.cs            # API contract
│   └── ProjectsController.cs
├── Tasks/                       # Task feature (same structure)
│   ├── Commands/
│   │   ├── Create/, Update/, Delete/
│   │   ├── ChangeStatus/, AddLabel/, RemoveLabel/
│   │   └── AssignMember/
│   ├── Queries/
│   │   ├── GetTask/, GetTasks/
│   └── TaskItemMapper.cs        # Shared entity → DTO mapping
└── Shared/                      # Cross-feature components
    ├── TaskDbContext.cs
    └── [Status, TaskType, Label, Comment models/DTOs]
```

**Navigation tip:** All code for a feature lives in its folder. New commands/queries go in their own subfolders.

## Adding New CQRS Operations

1. **Create Command/Query** in `Commands/{Operation}/` or `Queries/{Operation}/`
2. **Create Handler** implementing `IRequestHandler<TRequest, TResponse>` — inject `TaskDbContext` for DB access
3. **Create Event** (for commands) and optional EventHandler for side effects
4. **Add Controller endpoint** using `await _mediator.Send(command)`

**Example:** Adding `UpdateProject`:
- Create `Commands/Update/UpdateProjectCommand.cs`
- Create `Commands/Update/UpdateProjectCommandHandler.cs` — inject `TaskDbContext`, do the DB work directly
- Create `Commands/Update/ProjectUpdatedEvent.cs` and optional `ProjectUpdatedEventHandler.cs`
- Add `[HttpPut("{id}")]` endpoint in `ProjectsController.cs`

## External Integrations (GitHub, Azure DevOps)

External provider logic belongs in **EventHandlers**, not in command/query handlers. This keeps the core flow clean and makes integrations opt-in side effects.

**Example:** Syncing a created task to GitHub Issues:
- `CreateTaskCommandHandler` saves to DB and publishes `TaskCreatedEvent`
- A `GitHubTaskSyncEventHandler` (implementing `INotificationHandler<TaskCreatedEvent>`) calls the GitHub API

Refer to `.github/skills/github-issues-rest-api/` for GitHub API patterns.

## Testing Conventions

Use the **`Any` class** ([`UniTask.Tests/Any.cs`](UniTask.Tests/Any.cs)) for test data:
```csharp
// Generate random data
var project = Any.Project();
var task = Any.TaskItem(title: "Specific Title");  // Override only what matters

// Primitive values
var randomString = Any.String(10);
var randomInt = Any.Int(1, 100);
var randomEmail = Any.Email();
```

**Unit tests** instantiate handlers directly with an in-memory `TaskDbContext`. See [`LocalAdapterTests.cs`](UniTask.Tests/LocalAdapterTests.cs).

**Integration tests** use `CustomWebApplicationFactory` with in-memory database. See [`ProjectsControllerTests.cs`](UniTask.Tests/ProjectsControllerTests.cs).

**Why `Any`?** 
- Focuses tests on behavior, not data
- Prevents coupling to specific test values
- Reduces boilerplate

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
2. **Events are for side effects** - Main operation result goes through command response; events handle notifications, logging, and external integrations
3. **Handlers own the DB logic** - CommandHandlers write to the DB; QueryHandlers read from the DB
4. **External integrations go in EventHandlers** - Don't call GitHub/Azure DevOps APIs from command or query handlers
5. **Each CQRS operation gets its own folder** - Don't cram multiple commands in one file/folder

## Frontend Integration

- **API Client**: Auto-generated TypeScript client from NSwag (see [`ClientApp/src/api/`](UniTask.Api/ClientApp/src/api/))
- **Components**: Reusable UI in [`ClientApp/src/components/ui/`](UniTask.Api/ClientApp/src/components/ui/) using Radix UI primitives
- **Styling**: TailwindCSS with custom config in [`tailwind.config.js`](UniTask.Api/ClientApp/tailwind.config.js)

## Future Backend Integrations

To add GitHub/Azure DevOps support:
1. Create an EventHandler implementing `INotificationHandler<TEvent>` for the relevant event(s)
2. Call the external API inside the handler
3. Refer to `.github/skills/github-issues-rest-api/` for GitHub API patterns

