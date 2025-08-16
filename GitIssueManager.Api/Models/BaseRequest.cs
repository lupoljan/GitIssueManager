namespace GitIssueManager.Api.Models
{
    public class BaseRequest
    {
        public string Token { get; set; }
        public string Owner { get; set; }
        public string Repo { get; set; }
    }
}
