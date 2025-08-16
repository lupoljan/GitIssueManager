using GitIssueManager.Core.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using System.Web;

namespace GitIssueManager.Tests.Services
{
    public class GitLabServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly GitLabService _gitLabService;
        private const string BaseUrl = "https://gitlab.com/api/v4";

        public GitLabServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_mockHandler.Object);
            _gitLabService = new GitLabService(httpClient);
        }

        [Fact]
        public async Task CreateIssueAsync_Success_ReturnsIssue()
        {
            // Arrange
            var token = "test_token";
            var owner = "test_owner";
            var repo = "test_repo";
            var title = "Test Title";
            var description = "Test Description";
            var projectId = HttpUtility.UrlEncode($"{owner}/{repo}");

            var responseIssue = new
            {
                iid = 123,
                title,
                description
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseIssue))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString() == $"{BaseUrl}/projects/{projectId}/issues" &&
                        req.Headers.Contains("PRIVATE-TOKEN")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _gitLabService.CreateIssueAsync(token, owner, repo, title, description);

            // Assert
            Assert.Equal("123", result.Id);
            Assert.Equal(title, result.Title);
            Assert.Equal(description, result.Description);
        }

        [Fact]
        public async Task UpdateIssueAsync_Success_ReturnsUpdatedIssue()
        {
            // Arrange
            var token = "test_token";
            var owner = "test_owner";
            var repo = "test_repo";
            var issueId = "456";
            var newTitle = "Updated Title";
            var newDescription = "Updated Description";
            var projectId = HttpUtility.UrlEncode($"{owner}/{repo}");

            var responseIssue = new
            {
                iid = int.Parse(issueId),
                title = newTitle,
                description = newDescription
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseIssue))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri.ToString() == $"{BaseUrl}/projects/{projectId}/issues/{issueId}"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _gitLabService.UpdateIssueAsync(
                token, owner, repo, issueId, newTitle, newDescription);

            // Assert
            Assert.Equal(issueId, result.Id);
            Assert.Equal(newTitle, result.Title);
            Assert.Equal(newDescription, result.Description);
        }

        [Fact]
        public async Task CloseIssueAsync_Success_ClosesIssue()
        {
            // Arrange
            var token = "test_token";
            var owner = "test_owner";
            var repo = "test_repo";
            var issueId = "789";
            var projectId = HttpUtility.UrlEncode($"{owner}/{repo}");

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri.ToString() == $"{BaseUrl}/projects/{projectId}/issues/{issueId}" &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"state_event\":\"close\"")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            await _gitLabService.CloseIssueAsync(token, owner, repo, issueId);

            // Assert (no exception means success)
        }

        [Fact]
        public async Task CloseIssueAsync_HandlesSpecialCharacters()
        {
            // Arrange
            var token = "test_token";
            var owner = "group/subgroup";
            var repo = "project@name";
            var issueId = "101";
            var encodedProjectId = HttpUtility.UrlEncode($"{owner}/{repo}");

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains(encodedProjectId)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            await _gitLabService.CloseIssueAsync(token, owner, repo, issueId);

            // Assert
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}