using GitIssueManager.Core.Exceptions;
using GitIssueManager.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GitIssueManager.Core.Services
{
    public class GitHubService : IGitService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "https://api.github.com";

        public async Task<GitIssue> CreateIssueAsync(string token, string owner, string repo, string title, string description)
        {
            var url = $"{BaseUrl}/repos/{owner}/{repo}/issues";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            AddGitHubHeaders(request, token);

            var body = new { title, body = description };
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            return await SendGitHubIssueRequest(request);
        }

        public async Task<GitIssue> UpdateIssueAsync(string token, string owner, string repo, string issueId, string title, string description)
        {
            var url = $"{BaseUrl}/repos/{owner}/{repo}/issues/{issueId}";
            using var request = new HttpRequestMessage(HttpMethod.Patch, url);

            AddGitHubHeaders(request, token);

            var body = new { title, body = description };
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            return await SendGitHubIssueRequest(request);
        }

        public async Task CloseIssueAsync(string token, string owner, string repo, string issueId)
        {
            var url = $"{BaseUrl}/repos/{owner}/{repo}/issues/{issueId}";
            using var request = new HttpRequestMessage(HttpMethod.Patch, url);

            AddGitHubHeaders(request, token);

            var body = new { state = "closed" };
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            await SendGitHubIssueRequest(request);
        }

        private void AddGitHubHeaders(HttpRequestMessage request, string token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("User-Agent", "GitIssueManager");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
        }

        private async Task<GitIssue> SendGitHubIssueRequest(HttpRequestMessage request)
        {
            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new GitServiceException(content, (int)response.StatusCode);

            return JsonSerializer.Deserialize<GitIssue>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new GitHubIssueConverter() }
            });
        }
    }

    // Custom converter for GitHub issue response
    internal class GitHubIssueConverter : System.Text.Json.Serialization.JsonConverter<GitIssue>
    {
        public override GitIssue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            return new GitIssue
            {
                Id = root.GetProperty("number").ToString(),
                Title = root.GetProperty("title").GetString(),
                Description = root.GetProperty("body").GetString()
            };
        }

        public override void Write(Utf8JsonWriter writer, GitIssue value, JsonSerializerOptions options)
        {
            // Not needed for this use case
            throw new NotImplementedException();
        }
    }
}