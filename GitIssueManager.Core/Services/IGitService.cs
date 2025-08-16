using GitIssueManager.Core.Models;

namespace GitIssueManager.Core.Services
{
    public interface IGitService
    {
        Task<GitIssue> CreateIssueAsync(string token, string owner, string repo, string title, string description);
        Task<GitIssue> UpdateIssueAsync(string token, string owner, string repo, string issueId, string title, string description);
        Task CloseIssueAsync(string token, string owner, string repo, string issueId);
    }
}
