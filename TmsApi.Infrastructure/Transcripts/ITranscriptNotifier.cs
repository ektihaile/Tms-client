namespace TmsApi.Application.Transcripts;

public interface ITranscriptNotifier
{
    Task NotifyReadyAsync(
        int studentId,
        string reportId,
        string downloadUrl,
        CancellationToken ct = default);
}