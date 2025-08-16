using GitIssueManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Core.Factories
{
    public class GitServiceFactory
    {
        public IGitService CreateGitService(string serviceName)
        {
            return serviceName.ToLower() switch
            {
                "github" => new GitHubService(),
                "gitlab" => new GitLabService(),
                _ => throw new ArgumentException("Unsupported service")
            };
        }
    }
}
