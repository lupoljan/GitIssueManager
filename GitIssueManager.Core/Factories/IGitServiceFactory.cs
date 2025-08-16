using GitIssueManager.Core.Services;

namespace GitIssueManager.Core.Factories
{
    public interface IGitServiceFactory
    {
        IGitService CreateGitService(string serviceName);
    }
}
