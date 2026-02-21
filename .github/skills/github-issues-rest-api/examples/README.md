# GitHub Issues REST API - Examples

This folder contains comprehensive examples for using the GitHub REST API to work with issues, pull requests, and webhooks.

## Overview

The GitHub REST API provides programmatic access to GitHub issues, allowing you to automate workflows, integrate with external tools, and build custom applications.

All examples use:
- **API Version**: `2022-11-28`
- **Base URL**: `https://api.github.com`
- **Format**: JSON

## Files in This Folder

### [01-authentication.json](./01-authentication.json)
Authentication methods and security practices.

**Contents:**
- Personal Access Tokens (fine-grained and classic)
- GitHub Apps authentication
- OAuth Apps
- GITHUB_TOKEN for GitHub Actions
- Rate limiting information
- Security best practices
- Example code for each authentication method

**Use this when:** You need to authenticate API requests or choose an authentication method for your application.

---

### [02-list-issues.json](./02-list-issues.json)
Listing and filtering issues.

**Contents:**
- List repository issues with query parameters
- List issues assigned to authenticated user
- List organization issues
- Pagination handling
- Filtering examples (by state, labels, assignee, milestone, etc.)
- Sorting options

**Use this when:** You need to retrieve multiple issues with specific criteria.

---

### [03-create-issue.json](./03-create-issue.json)
Creating new issues.

**Contents:**
- Create issue endpoint details
- Request body parameters (title, body, labels, assignees, milestone)
- Real-world examples (bug reports, feature requests, tasks)
- Markdown formatting in issue bodies
- Validation errors and troubleshooting
- Code examples in multiple languages

**Use this when:** You need to create issues programmatically (automated bug reports, task creation, etc.).

---

### [04-update-issue.json](./04-update-issue.json)
Updating and managing existing issues.

**Contents:**
- Update issue endpoint (title, body, state, labels, assignees, milestone)
- Common operations (close, reopen, assign, label management)
- State reasons (completed, not_planned, reopened)
- Label management (add, remove, replace)
- Assignee management
- Lock/unlock conversations
- Partial vs full updates

**Use this when:** You need to modify issues (close bugs, assign tasks, update labels, etc.).

---

### [05-webhooks.json](./05-webhooks.json)
Setting up and managing webhooks for real-time notifications.

**Contents:**
- Creating repository and organization webhooks
- Available events (issues, issue_comment, projects_v2, etc.)
- Webhook payload structure and examples
- Security (signature validation with HMAC SHA-256)
- Webhook server implementation examples (Node.js, Python)
- Managing webhook deliveries
- Projects v2 webhook examples
- Troubleshooting and best practices

**Use this when:** You need real-time notifications about issue changes (CI/CD triggers, Slack notifications, external integrations).

---

### [06-additional-operations.json](./06-additional-operations.json)
Additional operations for working with issues.

**Contents:**
- Get single issue details
- Comments API (list, create, update, delete)
- Issue events and timeline
- Reactions (emoji reactions on issues and comments)
- Search issues (advanced search with qualifiers)
- Permission checking (assignees, collaborators)
- Complete workflow examples

**Use this when:** You need to work with issue comments, search for issues, or track issue history.

---

## Quick Start

### 1. Authenticate

Choose an authentication method from [01-authentication.json](./01-authentication.json):

```bash
# Using Personal Access Token
export GITHUB_TOKEN="your_pat_here"

# Test authentication
curl -L \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  https://api.github.com/user
```

### 2. List Issues

```bash
curl -L \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  "https://api.github.com/repos/OWNER/REPO/issues?state=open&labels=bug"
```

### 3. Create an Issue

```bash
curl -L \
  -X POST \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  https://api.github.com/repos/OWNER/REPO/issues \
  -d '{"title":"Found a bug","body":"Description here","labels":["bug"]}'
```

### 4. Update an Issue

```bash
curl -L \
  -X PATCH \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  https://api.github.com/repos/OWNER/REPO/issues/123 \
  -d '{"state":"closed","state_reason":"completed"}'
```

## Common Use Cases

### Automated Bug Reporting

1. Use authentication from file 01
2. Create issue with file 03 examples
3. Auto-assign using file 04 patterns

### Issue Workflow Automation

1. Set up webhook from file 05
2. Listen for `issues` event with action `opened`
3. Auto-label or assign based on title/body
4. Update issue using file 04 operations

### External Tool Integration

1. Set up webhook for issue changes
2. Parse webhook payload (file 05)
3. Sync with external ticketing system
4. Update issue when external status changes (file 04)

### Project Dashboard

1. List all issues using file 02 filters
2. Get issue details from file 06
3. Fetch comments and reactions (file 06)
4. Display in custom dashboard

### Automated Testing Integration

1. GitHub Actions workflow triggers on push
2. Tests fail → create issue (file 03)
3. Get test output as issue comment (file 06)
4. Close issue when tests pass (file 04)

## API Rate Limits

- **Authenticated**: 5,000 requests/hour
- **Unauthenticated**: 60 requests/hour
- **Search API**: 30 requests/minute (authenticated)

Check rate limit:
```bash
curl -L \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  https://api.github.com/rate_limit
```

## Response Headers

Every API response includes:
- `X-RateLimit-Limit`: Total requests allowed
- `X-RateLimit-Remaining`: Requests remaining
- `X-RateLimit-Reset`: Unix timestamp when limit resets
- `Link`: Pagination links (when applicable)

## Error Handling

Common HTTP status codes:

| Code | Meaning | Common Cause |
|------|---------|--------------|
| 200 | OK | Request successful |
| 201 | Created | Resource created successfully |
| 204 | No Content | Successful deletion |
| 304 | Not Modified | Cached response still valid |
| 400 | Bad Request | Invalid JSON or parameters |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | Insufficient permissions or rate limit |
| 404 | Not Found | Resource doesn't exist or no access |
| 410 | Gone | Resource permanently deleted |
| 422 | Validation Failed | Invalid field values |
| 500 | Internal Server Error | GitHub server error |

## Best Practices

### Security
- Never commit tokens to version control
- Use environment variables or secrets management
- Set minimal required permissions (fine-grained tokens)
- Rotate tokens regularly
- Validate webhook signatures

### Performance
- Use conditional requests (`If-None-Match` header)
- Implement pagination for large datasets
- Cache responses when appropriate
- Respect rate limits

### Reliability
- Handle errors gracefully
- Implement exponential backoff for retries
- Process webhook payloads asynchronously
- Monitor API usage and errors

## Programming Language Examples

All files include examples in:
- **curl** (command line)
- **Python** (requests library)
- **JavaScript/Node.js** (fetch API)

## Additional Resources

- [GitHub REST API Documentation](https://docs.github.com/en/rest)
- [GitHub Webhooks Guide](https://docs.github.com/en/webhooks)
- [GitHub OAuth Apps](https://docs.github.com/en/apps/oauth-apps)
- [GitHub Apps Documentation](https://docs.github.com/en/apps)
- [Fine-grained Personal Access Tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-fine-grained-personal-access-token)

## Support

For issues with these examples or the GitHub API:
1. Check the [GitHub REST API Status](https://www.githubstatus.com/)
2. Review [GitHub API Change Log](https://github.blog/changelog/label/api/)
3. Post in [GitHub Community Discussions](https://github.com/orgs/community/discussions/categories/api-and-webhooks)

## License

These examples are provided as-is for educational purposes. GitHub API usage is subject to [GitHub's Terms of Service](https://docs.github.com/en/site-policy/github-terms/github-terms-of-service).
