namespace GitIssueManager.Api.Models
{
    public class UpdateIssueRequest : CreateIssueRequest
    {
        public string IssueId { get; set; }
    }
}
