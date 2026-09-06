using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Tags("Enrollments V1")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsV1Controller(
    IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet(Name = "ListAllEnrollments")]
    [EndpointSummary("List all enrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEnrollments(CancellationToken ct)
    {
        var result = await enrollmentService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/approve")]
    [EndpointSummary("Approve an enrollment")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveEnrollment(
        int id,
        CancellationToken ct)
    {
        // TODO: Implement approval logic
        return Ok();
    }
}
