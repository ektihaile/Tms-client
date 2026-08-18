using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        // Check idempotency key
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing =
                await statusStore.GetReportIdForIdempotencyKeyAsync(
                    idempotencyKey,
                    ct);

            if (existing is not null)
            {
                var existingStatus =
                    await statusStore.GetAsync(existing, ct);

                Response.Headers.RetryAfter = "5";

                return Accepted(
                    Url.Action(nameof(GetStatus), new { id = existing }),
                    existingStatus);
            }
        }

        // Create a new report
        var reportId = Guid.NewGuid().ToString("N")[..12];

        var status = await statusStore.CreateAsync(
            reportId,
            request.StudentId,
            ct);

        // Link idempotency key
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await statusStore.LinkIdempotencyKeyAsync(
                idempotencyKey,
                reportId,
                ct);
        }

        // Queue the request
        await channel.Writer.WriteAsync(
            request.WithReportId(reportId),
            ct);

        Response.Headers.RetryAfter = "5";

        return Accepted(
            Url.Action(nameof(GetStatus), new { id = reportId }),
            status);
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(
        string id,
        CancellationToken ct)
    {
        var status = await statusStore.GetAsync(id, ct);

        if (status is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Transcript not found",
                Detail = $"No transcript request with id '{id}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(status);
    }
}