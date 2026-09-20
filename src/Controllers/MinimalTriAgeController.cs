using IssueTriage.src.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
namespace IssueTriage.src.Controllers;

using IssueTriage.src.Contracts;
using IssueTriage.src.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/minimal-triage")]
public class MinimalTriageController(MinimalTriageService minimalTriageService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Triage([FromBody] MinimalTriageRequest request , CancellationToken cancellationToken)
    {
        var response = await minimalTriageService.GetAnswer(request.Issue , cancellationToken);
        return Ok(response);
    }
}