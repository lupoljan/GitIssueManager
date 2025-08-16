namespace GitIssueManager.Api.Models
{
    public class CloseIssueRequest : BaseRequest
    {
        public string IssueId { get; set; }
    }
}