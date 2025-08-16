namespace GitIssueManager.Api.Models
{
    public class CreateIssueRequest : BaseRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
