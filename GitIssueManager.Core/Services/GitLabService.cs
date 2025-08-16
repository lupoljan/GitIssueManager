using GitIssueManager.Core.Exceptions;
using GitIssueManager.Core.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace GitIssueManager.Core.Services
{
    public class GitLabService : IGitService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://gitlab.com/api/v4";

        public GitLabService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GitIssue> CreateIssueAsync(string token, string owner, string repo, string title, string description)
        {
            var projectId = HttpUtility.UrlEncode($"{owner}/{repo}");
            var url = $"{BaseUrl}/projects/{projectId}/issues";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            AddGitLabHeaders(request, token);

            var body = new { title, description };
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            return await SendGitLabIssueRequest(request);
        }

        public async Task<GitIssue> UpdateIssueAsync(string token, string owner, string repo, string issueId, string title, string description)
        {
            var projectId = HttpUtility.UrlEncode($"{owner}/{repo}");
            var url = $"{BaseUrl}/projects/{projectId}/issues/{issueId}";
            using var request = new HttpRequestMessage(HttpMethod.Put, url);

            AddGitLabHeaders(request, token);

            var body = new { title, description };
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            return await SendGitLabIssueRequest(request);
        }

        public async Task CloseIssueAsync(string token, string owner, string repo, string issueId)
        {
            var projectId = HttpUtility.UrlEncode($"{owner}/{repo}");
            var url = $"{BaseUrl}/projects/{projectId}/issues/{issueId}";
            using var request = new HttpRequestMessage(HttpMethod.Patch, url);

            AddGitLabHeaders(request, token);

            var body = new { state_event = "close" };
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            await SendGitLabIssueRequest(request);
        }

        private void AddGitLabHeaders(HttpRequestMessage request, string token)
        {
            request.Headers.Add("PRIVATE-TOKEN", token);
        }

        private async Task<GitIssue> SendGitLabIssueRequest(HttpRequestMessage request)
        {
            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new GitServiceException(content, (int)response.StatusCode);

            return JsonSerializer.Deserialize<GitIssue>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new GitLabIssueConverter() }
            });
        }
    }

    // Custom converter for GitLab issue response
    internal class GitLabIssueConverter : System.Text.Json.Serialization.JsonConverter<GitIssue>
    {
        public override GitIssue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            return new GitIssue
            {
                Id = root.GetProperty("iid").ToString(),
                Title = root.GetProperty("title").GetString(),
                Description = root.GetProperty("description").GetString()
            };
        }

        public override void Write(Utf8JsonWriter writer, GitIssue value, JsonSerializerOptions options)
        {
            // Not needed for this use case
            throw new NotImplementedException();
        }
    }
}