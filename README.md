# Git Issue Manager

Git Issue Manager is a REST API application that allows users to manage issues across multiple Git hosting services. It currently supports GitHub and GitLab, with architecture designed for easy integration of additional services like Bitbucket.

# Features
- Cross-Platform Issue Management: Create, update, and close issues on GitHub and GitLab
- RESTful API: Standardized endpoints for all operations
- Extensible Architecture: Designed for easy addition of new Git services
- Comprehensive Error Handling: Detailed error responses with service-specific messages

# Prerequisites
- .NET 6 SDK
- Git hosting account (GitHub or GitLab)
- Personal access token with repository permissions

#  API Documentation
Interactive API documentation is available through Swagger UI

 ## API Reference
Base URL https://localhost:5001/api/issues

 ## Authentication
Include your Git service token in the request body for all operations.

 ## Endpoints

 ### Create Issue

 http
```
 POST /{service}
 ```
 json
```
 {
  "token": "your_git_token",
  "owner": "repository_owner",
  "repo": "repository_name",
  "title": "Issue title",
  "description": "Issue description"
}
```

 ### Update  Issue

  http
```
PUT /{service}/{issueId}
 ```
 json
```
{
  "token": "your_git_token",
  "owner": "repository_owner",
  "repo": "repository_name",
  "title": "Updated title",
  "description": "Updated description"
}
```

 ### Close Issue

  http
```
PATCH /{service}/{issueId}/close
 ```
 json
```
{
  "token": "your_git_token",
  "owner": "repository_owner",
  "repo": "repository_name",
  "issueId": "issue_to_close"
}
```

# Examples

## Create Issue (GitHub)

```
curl -X POST "https://localhost:5001/api/issues/github" \
  -H "Content-Type: application/json" \
  -d '{
    "token": "ghp_your_token",
    "owner": "your-org",
    "repo": "your-repo",
    "title": "New Bug",
    "description": "Critical bug description"
  }'
  ```

## Update Issue (GitLab)

```
curl -X PUT "https://localhost:5001/api/issues/gitlab/123" \
  -H "Content-Type: application/json" \
  -d '{
    "token": "glpat_your_token",
    "owner": "your-group",
    "repo": "your-project",
    "title": "Updated Title",
    "description": "Updated description"
  }'
  ```

## Close Issue (GitHub)

```
curl -X PATCH "https://localhost:5001/api/issues/github/456/close" \
  -H "Content-Type: application/json" \
  -d '{
    "token": "ghp_your_token",
    "owner": "your-org",
    "repo": "your-repo",
    "issueId": "456"
  }'
  ```
