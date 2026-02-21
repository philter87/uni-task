---
name: github-issues-rest-api
description: This skill allows you to interact with GitHub's Issues REST API to manage issues in your repositories. You can create, read, update, and delete issues, as well as list all issues in a repository. It should also know how to authenticate with GitHub in different ways.
---

# GitHub Issues REST API Skill

## Authentication Methods

GitHub REST API supports several authentication methods:

### 1. Personal Access Tokens (PAT)

#### Fine-Grained Personal Access Tokens (Recommended)
- More secure with granular permissions
- Can be scoped to specific repositories
- Requires specific permissions for each endpoint
- Created at: Settings → Developer settings → Personal access tokens → Fine-grained tokens

Required permissions for Issues API:
- **Read**: "Issues" repository permissions (read)
- **Write**: "Issues" repository permissions (write)

#### Personal Access Tokens (Classic)
- Broader scopes (less granular)
- Works with `repo` or `public_repo` scope
- Legacy method, not recommended for new implementations

**Usage Example:**
```bash
curl -L \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer YOUR_PERSONAL_ACCESS_TOKEN" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  https://api.github.com/repos/OWNER/REPO/issues
```

### 2. GitHub Apps

#### Overview
- Most powerful and flexible authentication method
- Recommended for integrations and automation
- Can act on behalf of a user or as the app itself
- Two types of tokens:
  - **User access tokens**: Act on behalf of a user
  - **Installation access tokens**: Act as the app

#### Authentication Flow
1. Create a GitHub App
2. Install the app on repositories
3. Generate JWT (JSON Web Token) using app credentials
4. Exchange JWT for installation access token

Required permissions for Issues API:
- "Issues" repository permissions (read/write)

**Usage Example:**
```bash
curl -L \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer INSTALLATION_ACCESS_TOKEN" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  https://api.github.com/repos/OWNER/REPO/issues
```

### 3. OAuth Apps

- User-facing applications
- Requires OAuth flow for user authorization
- Uses `client_id` and `client_secret`
- Less recommended than GitHub Apps for new projects

### 4. GitHub Actions (GITHUB_TOKEN)

When running in GitHub Actions workflows:
- Built-in `GITHUB_TOKEN` automatically provided
- Scoped to the repository
- Permissions can be configured in workflow

**Usage Example:**
```yaml
- name: Create issue via API
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
    gh api /repos/OWNER/REPO/issues \
      -X POST \
      -f title="Issue from workflow" \
      -f body="Created automatically"
```

### 5. Basic Authentication (Deprecated)

**NOT SUPPORTED** - Username and password authentication is no longer available.

### SAML SSO Considerations

For organizations using SAML SSO:
- Personal access tokens (classic) must be authorized after creation
- Fine-grained tokens are authorized during creation
- Apps may need additional authorization

## API Rate Limits

- **Authenticated requests**: 5,000 requests per hour
- **Unauthenticated requests**: 60 requests per hour
- **GraphQL API**: 5,000 points per hour

## Base URL

All API requests use: `https://api.github.com`

## Common Headers

```
Accept: application/vnd.github+json
Authorization: Bearer YOUR_TOKEN
X-GitHub-Api-Version: 2022-11-28
```

## Webhooks for Issues and Projects

Webhooks allow you to receive HTTP POST payloads whenever specific events occur.

### Issues Webhook Events

The `issues` event is triggered when there is activity relating to an issue:

**Available Actions:**
- `assigned` - Issue was assigned to a user
- `unassigned` - Issue was unassigned
- `opened` - Issue was opened
- `edited` - Issue was edited
- `closed` - Issue was closed
- `reopened` - Issue was reopened
- `labeled` - Label was added
- `unlabeled` - Label was removed
- `milestoned` - Issue was added to milestone
- `demilestoned` - Issue was removed from milestone
- `locked` - Issue was locked
- `unlocked` - Issue was unlocked
- `transferred` - Issue was transferred
- `deleted` - Issue was deleted

### Projects Webhook Events

#### Classic Projects (project, project_card, project_column)
- `project` - Activity on a classic project
- `project_card` - Activity on project cards
- `project_column` - Activity on project columns

#### Projects v2 (projects_v2, projects_v2_item)
- `projects_v2` - Activity on organization-level projects
- `projects_v2_item` - Activity on project items
- `projects_v2_status_update` - Activity on project status updates

**Available in:** Organizations only (for Projects v2)

### Setting Up Webhooks

1. Navigate to repository/organization settings
2. Go to Webhooks section
3. Click "Add webhook"
4. Configure:
   - **Payload URL**: Your server endpoint
   - **Content type**: `application/json` (recommended)
   - **Secret**: For validating deliveries
   - **Events**: Select specific events or choose "Send me everything"

### Webhook Payload Structure

**Headers:**
- `X-GitHub-Event`: Event name (e.g., "issues")
- `X-GitHub-Delivery`: Unique delivery ID
- `X-Hub-Signature-256`: HMAC signature for validation
- `X-GitHub-Hook-ID`: Webhook ID

**Example Payload (issues event):**
```json
{
  "action": "opened",
  "issue": {
    "number": 1,
    "title": "Found a bug",
    "body": "Description here",
    "state": "open",
    "user": {...},
    "labels": [...],
    "assignees": [...]
  },
  "repository": {...},
  "sender": {...}
}
```

### Securing Webhooks

Always validate webhook deliveries using the secret:

```python
import hmac
import hashlib

def verify_signature(payload, signature, secret):
    expected = 'sha256=' + hmac.new(
        secret.encode(),
        payload.encode(),
        hashlib.sha256
    ).hexdigest()
    return hmac.compare_digest(expected, signature)
```

## Detailed Examples

This skill includes comprehensive JSON example files in the `examples/` folder:

### [examples/01-authentication.json](./examples/01-authentication.json)
- All authentication methods with code examples
- Token creation guides
- Rate limiting information
- Security best practices

### [examples/02-list-issues.json](./examples/02-list-issues.json)
- List repository issues with filters
- Query parameters (state, labels, assignee, milestone, etc.)
- Pagination handling
- Filtering examples

### [examples/03-create-issue.json](./examples/03-create-issue.json)
- Creating issues with all options
- Real-world examples (bug reports, features, tasks)
- Markdown formatting
- Validation error handling

### [examples/04-update-issue.json](./examples/04-update-issue.json)
- Updating issue fields
- Close/reopen issues
- Label and assignee management
- Lock/unlock conversations

### [examples/05-webhooks.json](./examples/05-webhooks.json)
- Setting up webhooks
- Available events and payloads
- Signature validation
- Server implementation examples

### [examples/06-additional-operations.json](./examples/06-additional-operations.json)
- Comments API
- Issue events and timeline
- Reactions
- Search API
- Complete workflow examples

**See [examples/README.md](./examples/README.md) for quick start guide and common use cases.**