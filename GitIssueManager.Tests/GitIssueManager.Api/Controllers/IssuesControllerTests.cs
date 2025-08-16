using FluentAssertions;
using GitIssueManager.Api.Controllers;
using GitIssueManager.Api.Models;
using GitIssueManager.Core.Exceptions;
using GitIssueManager.Core.Factories;
using GitIssueManager.Core.Models;
using GitIssueManager.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit.Sdk;

namespace GitIssueManager.Api.Tests.Controllers
{
    public class IssuesControllerTests
    {
        private readonly Mock<IGitService> _mockGitService = new();
        private readonly Mock<IGitServiceFactory> _mockFactory = new();
        private readonly IssuesController _controller;

        public IssuesControllerTests()
        {
            _mockFactory.Setup(f => f.CreateGitService(It.IsAny<string>()))
                .Returns(_mockGitService.Object);

            _controller = new IssuesController(_mockFactory.Object);
        }

        [Theory]
        [InlineData("github")]
        [InlineData("gitlab")]
        public async Task CreateIssue_ValidRequest_ReturnsCreatedIssue(string service)
        {
            // Arrange
            var request = new CreateIssueRequest
            {
                Token = "test_token",
                Owner = "test_owner",
                Repo = "test_repo",
                Title = "Test Issue",
                Description = "Test Description"
            };

            var expectedIssue = new GitIssue
            {
                Id = "123",
                Title = request.Title,
                Description = request.Description
            };

            _mockGitService.Setup(s => s.CreateIssueAsync(
                request.Token, request.Owner, request.Repo, request.Title, request.Description))
                .ReturnsAsync(expectedIssue);

            // Act
            var result = await _controller.CreateIssue(service, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expectedIssue);

            _mockFactory.Verify(f => f.CreateGitService(service), Times.Once);
        }

        [Theory]
        [InlineData("github")]
        [InlineData("gitlab")]
        public async Task CreateIssue_ServiceError_ReturnsErrorResponse(string service)
        {
            // Arrange
            var request = new CreateIssueRequest();
            var errorMessage = "Validation failed";

            _mockGitService.Setup(s => s.CreateIssueAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new GitServiceException(errorMessage, 422));

            // Act
            var result = await _controller.CreateIssue(service, request);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(422);

            result.As<ObjectResult>().Value.Should().Be(($"Git service error occurred (Status: {422}): {errorMessage}"), errorMessage);
        }

        [Theory]
        [InlineData("github")]
        [InlineData("gitlab")]
        public async Task UpdateIssue_ValidRequest_ReturnsUpdatedIssue(string service)
        {
            // Arrange
            var issueId = "456";
            var request = new UpdateIssueRequest
            {
                Token = "test_token",
                Owner = "test_owner",
                Repo = "test_repo",
                Title = "Updated Title",
                Description = "Updated Description"
            };

            var expectedIssue = new GitIssue
            {
                Id = issueId,
                Title = request.Title,
                Description = request.Description
            };

            _mockGitService.Setup(s => s.UpdateIssueAsync(
                request.Token, request.Owner, request.Repo, issueId, request.Title, request.Description))
                .ReturnsAsync(expectedIssue);

            // Act
            var result = await _controller.UpdateIssue(service, issueId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expectedIssue);
        }

        [Theory]
        [InlineData("github")]
        [InlineData("gitlab")]
        public async Task UpdateIssue_InvalidIssueId_ReturnsNotFound(string service)
        {
            // Arrange
            var issueId = "invalid_id";
            var request = new UpdateIssueRequest();
            var errorMessage = "Issue not found";

            _mockGitService.Setup(s => s.UpdateIssueAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), issueId, It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new GitServiceException(errorMessage, 404));

            // Act
            var result = await _controller.UpdateIssue(service, issueId, request);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(404);

            result.As<ObjectResult>().Value.Should().Be(($"Git service error occurred (Status: {404}): {errorMessage}"), errorMessage);
        }

        [Theory]
        [InlineData("github")]
        [InlineData("gitlab")]
        public async Task CloseIssue_ValidRequest_ReturnsNoContent(string service)
        {
            // Arrange
            var issueId = "789";
            var request = new CloseIssueRequest
            {
                Token = "test_token",
                Owner = "test_owner",
                Repo = "test_repo",
                IssueId = issueId
            };

            // Act
            var result = await _controller.CloseIssue(service, issueId, request);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            _mockGitService.Verify(s => s.CloseIssueAsync(
                request.Token, request.Owner, request.Repo, issueId), Times.Once);
        }

        [Theory]
        [InlineData("github")]
        [InlineData("gitlab")]
        public async Task CloseIssue_AlreadyClosed_ReturnsConflict(string service)
        {
            // Arrange
            var issueId = "789";
            var request = new CloseIssueRequest();

            _mockGitService.Setup(s => s.CloseIssueAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), issueId))
                .ThrowsAsync(new GitServiceException("Already closed", 409));

            // Act
            var result = await _controller.CloseIssue(service, issueId, request);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task CreateIssue_UnsupportedService_ReturnsBadRequest()
        {
            // Arrange
            var service = "unknown";
            var request = new CreateIssueRequest();

            _mockFactory.Setup(f => f.CreateGitService(service))
                .Throws<ArgumentException>();

            // Act
            var result = await _controller.CreateIssue(service, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateIssue_ServiceFactoryError_ReturnsBadRequest()
        {
            // Arrange
            var service = "unsupported";
            var issueId = "123";
            var request = new UpdateIssueRequest();

            _mockFactory.Setup(f => f.CreateGitService(service))
                .Throws<ArgumentException>();

            // Act
            var result = await _controller.UpdateIssue(service, issueId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CloseIssue_InvalidService_ReturnsBadRequest()
        {
            // Arrange
            var service = "invalid_service";
            var issueId = "456";
            var request = new CloseIssueRequest();
            var expectedMessage = $"Unsupported service: {service}";

            _mockFactory.Setup(f => f.CreateGitService(service))
                .Throws(new ArgumentException(expectedMessage));

            // Act
            var result = await _controller.CloseIssue(service, issueId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("github")]
        [InlineData("gitlab")]
        public async Task UpdateIssue_ServiceException_ReturnsCorrectStatusCode(string service)
        {
            // Test various status codes
            var testCases = new[]
            {
        (statusCode: 400, error: "Bad request"),
        (statusCode: 401, error: "Unauthorized"),
        (statusCode: 403, error: "Forbidden"),
        (statusCode: 404, error: "Not found"),
        (statusCode: 409, error: "Conflict"),
        (statusCode: 422, error: "Validation failed")
    };

            foreach (var (statusCode, error) in testCases)
            {
                // Arrange
                var issueId = "123";
                var request = new UpdateIssueRequest();

                _mockGitService.Setup(s => s.UpdateIssueAsync(It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>(), issueId, It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new GitServiceException(error, statusCode));

                // Act
                var result = await _controller.UpdateIssue(service, issueId, request);

                // Assert
                result.Should().BeOfType<ObjectResult>()
                    .Which.StatusCode.Should().Be(statusCode);

                result.As<ObjectResult>().Value.Should().Be($"Git service error occurred (Status: {statusCode}): {error}",
                     because: "{1}", 
                     statusCode, error);
            }
        }

    }
}