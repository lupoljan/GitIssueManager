using GitIssueManager.Core.Factories;
using GitIssueManager.Core.Services;
using Moq;
using System;
using System.Net.Http;
using Xunit;

namespace GitIssueManager.Tests.Factories
{
    public class GitServiceFactoryTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly GitServiceFactory _factory;

        public GitServiceFactoryTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            // Setup mock to return a new HttpClient for any name
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(() => new HttpClient());

            _factory = new GitServiceFactory(_mockHttpClientFactory.Object);
        }

        [Theory]
        [InlineData("github", typeof(GitHubService))]
        [InlineData("GITHUB", typeof(GitHubService))]
        [InlineData("gitlab", typeof(GitLabService))]
        [InlineData("GITLAB", typeof(GitLabService))]
        public void CreateGitService_ValidServices_ReturnsCorrectType(string serviceName, Type expectedType)
        {
            // Act
            var service = _factory.CreateGitService(serviceName);

            // Assert
            Assert.IsType(expectedType, service);
            _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void CreateGitService_InvalidService_ThrowsException()
        {
            // Arrange
            var invalidService = "unknownservice";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _factory.CreateGitService(invalidService));
            Assert.Contains("Unsupported service", exception.Message);
            _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CreateGitService_NullServiceName_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _factory.CreateGitService(null));
            _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }
    }
}