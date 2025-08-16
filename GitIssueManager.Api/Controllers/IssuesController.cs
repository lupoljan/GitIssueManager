using GitIssueManager.Api.Models;
using GitIssueManager.Core.Factories;
using Microsoft.AspNetCore.Mvc;

namespace GitIssueManager.Api.Controllers
{
    [ApiController]
    [Route("api/issues")]
    public class IssuesController : ControllerBase
    {
        private readonly GitServiceFactory _serviceFactory;

        public IssuesController(GitServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        [HttpPost("{service}")]
        public async Task<IActionResult> CreateIssue(string service, [FromBody] CreateIssueRequest request)
        {
            try
            {
                var gitService = _serviceFactory.CreateGitService(service);
                var issue = await gitService.CreateIssueAsync(
                    request.Token,
                    request.Owner,
                    request.Repo,
                    request.Title,
                    request.Description
                );
                return Ok(issue);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{service}/{issueId}")]
        public async Task<IActionResult> UpdateIssue(string service, string issueId, [FromBody] UpdateIssueRequest request)
        {
            try
            {
                var gitService = _serviceFactory.CreateGitService(service);
                var issue = await gitService.UpdateIssueAsync(
                    request.Token,
                    request.Owner,
                    request.Repo,
                    issueId,
                    request.Title,
                    request.Description
                );
                return Ok(issue);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("{service}/{issueId}/close")]
        public async Task<IActionResult> CloseIssue(string service, string issueId, [FromBody] CloseIssueRequest request)
        {
            try
            {
                var gitService = _serviceFactory.CreateGitService(service);
                await gitService.CloseIssueAsync(
                    request.Token,
                    request.Owner,
                    request.Repo,
                    issueId
                );
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
