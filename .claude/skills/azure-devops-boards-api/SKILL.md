---
name: azure-devops-boards-api
description: This skill provides functionality to interact with the Azure DevOps Boards API, allowing users to manage work items, boards, and related resources programmatically. It includes methods for creating, reading, updating, and deleting work items, as well as listing boards and their associated work items. The skill also covers authentication methods for Azure DevOps API access. Especially, how to create app registrations and generate PATs for authentication.
---

# Azure DevOps Boards REST API Skill

## Overview

Azure DevOps Boards is a work tracking system that supports Agile methodologies (Scrum, Kanban, CMMI). It provides comprehensive tools for managing work items, sprints, backlogs, and boards.

### Key Concepts

- **Work Items**: Core units of work (User Stories, Tasks, Bugs, Features, Epics)
- **Projects**: Containers for work items and boards
- **Organizations**: Top-level containers for projects
- **Boards**: Visual representation of work items in different states
- **Backlogs**: Prioritized lists of work items
- **Sprints/Iterations**: Time-boxed periods for completing work
- **Areas**: Used to organize work items by team or feature area
- **Queries**: Saved searches for work items

## Authentication Methods

Azure DevOps REST API supports two primary authentication methods:

### 1. Personal Access Tokens (PAT) - Recommended

PATs are the simplest and most common authentication method.

#### Creating a PAT

1. Navigate to your Azure DevOps organization
2. Click on **User Settings** (top right) → **Personal access tokens**
3. Click **+ New Token**
4. Configure the token:
   - **Name**: Descriptive name for the token
   - **Organization**: Select organization or "All accessible organizations"
   - **Expiration**: Set expiration date (max 1 year, custom for fine-grained tokens)
   - **Scopes**: Select required permissions

#### Required Scopes for Boards API

- **Work Items (Read)**: Read work items, queries, and boards
- **Work Items (Write)**: Create, update, and delete work items
- **Project and Team (Read)**: Read project information
- **Analytics (Read)**: For advanced queries and reporting

#### Using PAT in API Calls

PATs use **Basic Authentication** with empty username:

```bash
# Encode PAT to Base64
echo -n ":YOUR_PAT" | base64

# Use in API call
curl -X GET \
  -H "Content-Type: application/json" \
  -H "Authorization: Basic OnlvdXJfcGF0X2hlcmU=" \
  "https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?api-version=7.1"
```

**Alternative (simpler):**
```bash
curl -u :YOUR_PAT \
  "https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?api-version=7.1"
```

### 2. OAuth 2.0 / Microsoft Entra ID (Azure AD)

For enterprise applications and multi-tenant scenarios.

#### Creating an App Registration

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to **Microsoft Entra ID** (formerly Azure AD) → **App registrations**
3. Click **+ New registration**
4. Configure the application:
   - **Name**: Application name
   - **Supported account types**: Choose based on your needs
   - **Redirect URI**: Web platform with your callback URL

#### Configure API Permissions

1. In your app registration, go to **API permissions**
2. Click **+ Add a permission**
3. Select **Azure DevOps**
4. Choose **Delegated permissions**
5. Select required scopes:
   - `user_impersonation`: Full access to Azure DevOps REST APIs

#### Certificate & Secrets

1. Go to **Certificates & secrets**
2. Click **+ New client secret**
3. Add description and expiration
4. Copy the secret value (only shown once!)

#### OAuth 2.0 Authorization Flow

**Step 1: Get Authorization Code**
```
https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize?
  client_id={client_id}
  &response_type=code
  &redirect_uri={redirect_uri}
  &response_mode=query
  &scope=499b84ac-1321-427f-aa17-267ca6975798/.default
  &state={state}
```

**Step 2: Exchange Code for Token**
```bash
curl -X POST \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id={client_id}" \
  -d "client_secret={client_secret}" \
  -d "code={authorization_code}" \
  -d "redirect_uri={redirect_uri}" \
  -d "grant_type=authorization_code" \
  "https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token"
```

**Step 3: Use Access Token**
```bash
curl -X GET \
  -H "Authorization: Bearer {access_token}" \
  -H "Content-Type: application/json" \
  "https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?api-version=7.1"
```

#### Service Principal Authentication

For automated scenarios without user interaction:

```bash
curl -X POST \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id={client_id}" \
  -d "client_secret={client_secret}" \
  -d "scope=499b84ac-1321-427f-aa17-267ca6975798/.default" \
  -d "grant_type=client_credentials" \
  "https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token"
```

**Note**: Azure DevOps resource ID is `499b84ac-1321-427f-aa17-267ca6975798`

## API Structure

### Base URL

```
https://dev.azure.com/{organization}/{project}/_apis/
```

**Alternative formats:**
- Old format: `https://{organization}.visualstudio.com/{project}/_apis/` (still supported)
- For organization-level APIs: `https://dev.azure.com/{organization}/_apis/`

### API Versioning

All API calls require an `api-version` parameter:

- **Current stable version**: `7.1`
- **Preview versions**: `7.1-preview.{n}` (may change)

Always specify the version:
```
?api-version=7.1
```

### Common Headers

```
Content-Type: application/json
Authorization: Basic {base64_encoded_pat}
Accept: application/json
```

For JSON Patch operations:
```
Content-Type: application/json-patch+json
```

## Work Item Types

Common work item types in Agile process template:

| Type | Description | Parent Type |
|------|-------------|-------------|
| **Epic** | Large body of work (multiple features) | None |
| **Feature** | Business outcome or capability | Epic |
| **User Story** | User requirement or feature from user perspective | Feature |
| **Task** | Unit of work to complete a user story | User Story |
| **Bug** | Issue or defect | User Story |
| **Issue** | Risk, impediment, or problem | None |
| **Test Case** | Validates functionality | User Story |

### Work Item States

Default states vary by type, but commonly include:

**User Story/Bug:**
- New
- Active
- Resolved
- Closed
- Removed

**Task:**
- To Do
- In Progress
- Done
- Removed

**Epic/Feature:**
- New
- Active
- Resolved
- Closed
- Removed

### System Fields

Common fields across all work item types:

| Field | Reference Name | Type | Description |
|-------|---------------|------|-------------|
| ID | System.Id | Integer | Unique identifier |
| Title | System.Title | String | Work item title |
| State | System.State | String | Current state |
| Assigned To | System.AssignedTo | Identity | Person assigned |
| Created By | System.CreatedBy | Identity | Creator |
| Created Date | System.CreatedDate | DateTime | Creation date |
| Changed Date | System.ChangedDate | DateTime | Last modified date |
| Description | System.Description | HTML | Detailed description |
| Tags | System.Tags | String | Semicolon-separated tags |
| Area Path | System.AreaPath | TreePath | Area classification |
| Iteration Path | System.IterationPath | TreePath | Iteration classification |
| Work Item Type | System.WorkItemType | String | Type name |

## Core API Operations

### 1. List Work Items by Query

**Endpoint:**
```
GET https://dev.azure.com/{organization}/{project}/_apis/wit/wiql?api-version=7.1
```

**Request Body (WIQL - Work Item Query Language):**
```json
{
  "query": "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.WorkItemType] = 'User Story' AND [State] <> 'Closed' ORDER BY [Changed Date] DESC"
}
```

**Response:**
```json
{
  "queryType": "flat",
  "queryResultType": "workItem",
  "asOf": "2026-02-21T10:30:00Z",
  "columns": [...],
  "workItems": [
    {"id": 123, "url": "..."},
    {"id": 124, "url": "..."}
  ]
}
```

### 2. Get Work Item Details

**Endpoint:**
```
GET https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/{id}?api-version=7.1
```

**Query Parameters:**
- `fields`: Comma-separated list of fields (optional)
- `$expand`: `all`, `relations`, `fields`, `links`, `none`

**Example:**
```bash
GET https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/123?fields=System.Title,System.State,System.AssignedTo&api-version=7.1
```

### 3. Get Multiple Work Items

**Endpoint:**
```
GET https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids={ids}&api-version=7.1
```

**Example:**
```bash
GET https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids=123,124,125&fields=System.Title,System.State&api-version=7.1
```

### 4. Create Work Item

**Endpoint:**
```
POST https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/${type}?api-version=7.1
```

**Headers:**
```
Content-Type: application/json-patch+json
```

**Request Body (JSON Patch format):**
```json
[
  {
    "op": "add",
    "path": "/fields/System.Title",
    "value": "Implement login feature"
  },
  {
    "op": "add",
    "path": "/fields/System.Description",
    "value": "<div>User should be able to log in with email and password</div>"
  },
  {
    "op": "add",
    "path": "/fields/System.AssignedTo",
    "value": "user@example.com"
  },
  {
    "op": "add",
    "path": "/fields/System.Tags",
    "value": "authentication; security"
  },
  {
    "op": "add",
    "path": "/fields/Microsoft.VSTS.Common.Priority",
    "value": 1
  }
]
```

**Example:**
```bash
curl -X POST \
  -u :YOUR_PAT \
  -H "Content-Type: application/json-patch+json" \
  -d '[{"op":"add","path":"/fields/System.Title","value":"New User Story"}]' \
  "https://dev.azure.com/{org}/{project}/_apis/wit/workitems/\$User Story?api-version=7.1"
```

### 5. Update Work Item

**Endpoint:**
```
PATCH https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/{id}?api-version=7.1
```

**Request Body:**
```json
[
  {
    "op": "replace",
    "path": "/fields/System.State",
    "value": "Active"
  },
  {
    "op": "add",
    "path": "/fields/System.History",
    "value": "Started working on this item"
  }
]
```

### 6. Delete Work Item

**Endpoint:**
```
DELETE https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/{id}?api-version=7.1
```

**Note**: This moves work item to "Recycle Bin", not permanent deletion.

**Permanent Delete:**
```
DELETE https://dev.azure.com/{organization}/{project}/_apis/wit/recyclebin/{id}?api-version=7.1
```

### 7. Add Link/Relation

**Update work item with relation:**
```json
[
  {
    "op": "add",
    "path": "/relations/-",
    "value": {
      "rel": "System.LinkTypes.Hierarchy-Forward",
      "url": "https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/{child_id}",
      "attributes": {
        "comment": "Making this a parent-child relationship"
      }
    }
  }
]
```

**Common Link Types:**
- `System.LinkTypes.Hierarchy-Forward`: Parent-Child (this is parent)
- `System.LinkTypes.Hierarchy-Reverse`: Child-Parent (this is child)
- `System.LinkTypes.Related`: Related work item
- `System.LinkTypes.Dependency-Forward`: Predecessor
- `System.LinkTypes.Dependency-Reverse`: Successor

### 8. Add Attachment

**Step 1: Upload File**
```
POST https://dev.azure.com/{organization}/{project}/_apis/wit/attachments?fileName={filename}&api-version=7.1
```

**Headers:**
```
Content-Type: application/octet-stream
```

**Response:**
```json
{
  "id": "guid",
  "url": "https://..."
}
```

**Step 2: Link Attachment to Work Item**
```json
[
  {
    "op": "add",
    "path": "/relations/-",
    "value": {
      "rel": "AttachedFile",
      "url": "https://dev.azure.com/{organization}/{project}/_apis/wit/attachments/{attachment_id}",
      "attributes": {
        "comment": "Screenshot of the issue"
      }
    }
  }
]
```

### 9. Add Comment

**Endpoint:**
```
POST https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/{id}/comments?api-version=7.1-preview.3
```

**Request Body:**
```json
{
  "text": "This is a comment on the work item"
}
```

**Alternative (using work item update):**
```json
[
  {
    "op": "add",
    "path": "/fields/System.History",
    "value": "This appears in the history/discussion"
  }
]
```

### 10. Get Boards

**Endpoint:**
```
GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards?api-version=7.1
```

**Response:**
```json
{
  "value": [
    {
      "id": "board-id",
      "name": "Stories",
      "url": "..."
    }
  ]
}
```

### 11. Get Board Columns

**Endpoint:**
```
GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/columns?api-version=7.1
```

### 12. Get Work Items on Board

**Endpoint:**
```
GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/rows?api-version=7.1
```

## WIQL (Work Item Query Language)

WIQL is a SQL-like query language for work items.

### Basic Syntax

```sql
SELECT [Field1], [Field2]
FROM WorkItems
WHERE [Condition]
ORDER BY [Field] ASC/DESC
```

### Example Queries

**All active bugs:**
```sql
SELECT [System.Id], [System.Title], [System.State]
FROM WorkItems
WHERE [System.WorkItemType] = 'Bug'
  AND [System.State] = 'Active'
ORDER BY [Microsoft.VSTS.Common.Priority] ASC
```

**User stories assigned to me:**
```sql
SELECT [System.Id], [System.Title]
FROM WorkItems
WHERE [System.WorkItemType] = 'User Story'
  AND [System.AssignedTo] = @Me
```

**Items changed in last 7 days:**
```sql
SELECT [System.Id], [System.Title], [System.ChangedDate]
FROM WorkItems
WHERE [System.ChangedDate] >= @Today - 7
ORDER BY [System.ChangedDate] DESC
```

**Work items with specific tag:**
```sql
SELECT [System.Id], [System.Title]
FROM WorkItems
WHERE [System.Tags] CONTAINS 'important'
```

### Macros

- `@Me`: Current user
- `@Today`: Today's date
- `@Project`: Current project
- `@CurrentIteration`: Current iteration/sprint

## JSON Patch Operations

Work item create and update operations use [JSON Patch](http://jsonpatch.com/) format.

### Operations

| Operation | Description | Example |
|-----------|-------------|---------|
| `add` | Add new field or relation | Add title, description, tags |
| `replace` | Update existing field | Change state, update title |
| `remove` | Remove field value or relation | Remove tag, delete relation |
| `test` | Test value before operation | Conditional updates |
| `copy` | Copy value from one field | Rarely used |
| `move` | Move value between fields | Rarely used |

### Path Formats

- Field: `/fields/System.Title`
- Relation (append): `/relations/-`
- Relation (specific): `/relations/0`
- Relation (remove): `/relations/2`

## Rate Limits and Best Practices

### Rate Limits

Azure DevOps uses **Token Bucket Algorithm**:

- **Threshold**: 200 requests per user per organization
- **Delay Bucket**: 200 requests capacity
- **Refill Rate**: Varies based on number of users

**Recommendations:**
- Implement exponential backoff on 429 (Too Many Requests)
- Batch operations when possible
- Cache frequently accessed data
- Use webhooks instead of polling

### Best Practices

1. **Use specific fields in queries**: Don't fetch all fields if you only need a few
2. **Batch work item fetches**: Use `workitems?ids=1,2,3` instead of individual calls
3. **Leverage WIQL**: Server-side filtering is more efficient
4. **Use $expand wisely**: Only expand relations/links when needed
5. **Implement retry logic**: Handle transient failures
6. **Version your API calls**: Always specify api-version
7. **Secure PATs**: Treat like passwords, rotate regularly, use minimum required scopes

## Webhooks and Service Hooks

Azure DevOps supports webhooks through **Service Hooks**.

### Setting Up Service Hooks

1. Go to Project Settings → Service Hooks
2. Click **+** to create subscription
3. Choose service (Web Hooks, Azure Functions, etc.)
4. Configure trigger event
5. Set up action (URL, authentication)

### Available Events

**Work Item Events:**
- Work item created
- Work item updated
- Work item deleted
- Work item restored
- Work item commented on

**Git Events:**
- Code pushed
- Pull request created/updated/merged
- Build completed
- Release deployment started/completed

### Webhook Payload Example

**Work Item Created:**
```json
{
  "subscriptionId": "...",
  "notificationId": 1,
  "id": "...",
  "eventType": "workitem.created",
  "publisherId": "tfs",
  "message": {
    "text": "Bug #123 (New bug) created by User Name"
  },
  "detailedMessage": {...},
  "resource": {
    "id": 123,
    "rev": 1,
    "fields": {
      "System.Title": "New bug",
      "System.WorkItemType": "Bug",
      "System.State": "New"
    },
    "url": "..."
  },
  "resourceVersion": "1.0",
  "createdDate": "2026-02-21T10:30:00Z"
}
```

### Authenticating Webhook Requests

Use **Basic Authentication** in service hook configuration:
- Username: Can be anything or empty
- Password: Shared secret

Validate received requests by checking the secret.

## Error Handling

### Common Error Codes

| Code | Description | Solution |
|------|-------------|----------|
| 401 | Unauthorized | Check PAT/token validity and scopes |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Verify organization/project/work item ID |
| 429 | Too Many Requests | Implement backoff and retry |
| 400 | Bad Request | Validate request body and parameters |
| 203 | Non-Authoritative | PAT expired, need to recreate |

### Example Error Response

```json
{
  "$id": "1",
  "innerException": null,
  "message": "VS402337: The work item does not exist, or you do not have permission to access it.",
  "typeName": "Microsoft.TeamFoundation.WorkItemTracking.Server.WorkItemNotFoundException",
  "typeKey": "WorkItemNotFoundException",
  "errorCode": 0,
  "eventId": 3000
}
```

## Additional Resources

- [Azure DevOps REST API Documentation](https://learn.microsoft.com/en-us/rest/api/azure/devops/)
- [Work Item Tracking REST API](https://learn.microsoft.com/en-us/rest/api/azure/devops/wit/)
- [Boards REST API](https://learn.microsoft.com/en-us/rest/api/azure/devops/work/)
- [Service Hooks Documentation](https://learn.microsoft.com/en-us/azure/devops/service-hooks/)
- [WIQL Syntax Reference](https://learn.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax)