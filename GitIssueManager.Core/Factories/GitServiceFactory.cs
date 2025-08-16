using GitIssueManager.Core.Services;

namespace GitIssueManager.Core.Factories
{
    public class GitServiceFactory : IGitServiceFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GitServiceFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public virtual IGitService CreateGitService(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentNullException(nameof(serviceName));

            var normalizedService = serviceName.ToLower();
            if (normalizedService != "github" && normalizedService != "gitlab")
            {
                throw new ArgumentException($"Unsupported service: {serviceName}");
            }

            var httpClient = _httpClientFactory.CreateClient(serviceName);

            return serviceName.ToLower() switch
            {
                "github" => new GitHubService(httpClient),
                "gitlab" => new GitLabService(httpClient),
                _ => throw new ArgumentException($"Unsupported service: {serviceName}")
            };
        }
    }
}

