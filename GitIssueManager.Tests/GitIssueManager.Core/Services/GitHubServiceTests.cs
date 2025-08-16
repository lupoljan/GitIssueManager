using GitIssueManager.Core.Exceptions;
using GitIssueManager.Core.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GitIssueManager.Tests.Services
{
    public class GitHubServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly GitHubService _gitHubService;
        private const string BaseUrl = "https://api.github.com";

        public GitHubServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_mockHandler.Object);
            _gitHubService = new GitHubService(httpClient);
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

            var responseIssue = new
            {
                number = 123,
                title,
                body = description
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(JsonSerializer.Serialize(responseIssue),
                    Encoding.UTF8, "application/json")
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString() == $"{BaseUrl}/repos/{owner}/{repo}/issues" &&
                        req.Headers.Authorization.Parameter == token),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _gitHubService.CreateIssueAsync(token, owner, repo, title, description);

            // Assert
            Assert.Equal("123", result.Id);
            Assert.Equal(title, result.Title);
            Assert.Equal(description, result.Description);
        }

        [Fact]
        public async Task CreateIssueAsync_Failure_ThrowsException()
        {
            // Arrange
            var token = "test_token";
            var owner = "test_owner";
            var repo = "test_repo";
            var title = "Test Title";
            var description = "Test Description";

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("{\"message\":\"Not Found\"}")
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<GitServiceException>(
                () => _gitHubService.CreateIssueAsync(token, owner, repo, title, description));

            Assert.Equal(404, exception.StatusCode);
            Assert.Contains("Not Found", exception.ErrorContent);
        }

        [Fact]
        public async Task CloseIssueAsync_Success_ClosesIssue()
        {
            // Arrange
            var token = "test_token";
            var owner = "test_owner";
            var repo = "test_repo";
            var issueId = "789";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Patch &&
                        req.RequestUri.ToString() == $"{BaseUrl}/repos/{owner}/{repo}/issues/{issueId}" &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"state\":\"closed\"")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            await _gitHubService.CloseIssueAsync(token, owner, repo, issueId);
        }
    }
}