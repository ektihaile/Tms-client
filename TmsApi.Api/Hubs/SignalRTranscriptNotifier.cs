using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Transcripts;

namespace TmsApi.Api.Hubs;

public class SignalRTranscriptNotifier(
    IHubContext<TmsHub, ITmsHubClient> hubContext)
    : ITranscriptNotifier
{
    public Task NotifyReadyAsync(
        int studentId,
        string reportId,
        string downloadUrl,
        CancellationToken ct = default)
    {
        return hubContext
            .Clients
            .Group($"student:{studentId}")
            .ReceiveTranscriptReady(reportId, downloadUrl);
    }
}