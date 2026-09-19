using IssueTriage.src.Contracts;
using IssueTriage.src.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IssueTriage.src.Controllers;
[ApiController]
[Route("/api/function-calling")]
public class FunctionCallingController(FunctionCallingService service) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> Triage(
        [FromBody] MinimalTriageRequest request, CancellationToken cancellationToken
    )
    {
        var result = await service.TriageIssue(request.Issue, cancellationToken);
        return Ok(result);
    }
}
