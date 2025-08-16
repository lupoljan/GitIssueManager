namespace GitIssueManager.Core.Exceptions
{
    public class GitServiceException : Exception
    {
        public int StatusCode { get; }
        public string ErrorContent { get; }

        public GitServiceException() { }

        public GitServiceException(string message)
            : base(message) { }

        public GitServiceException(string message, Exception innerException)
            : base(message, innerException) { }

        // Specialized constructor for HTTP error responses
        public GitServiceException(string errorContent, int statusCode = 0)
            : base($"Git service error occurred (Status: {statusCode}): {errorContent}")
        {
            StatusCode = statusCode;
            ErrorContent = errorContent;
        }
    }
}