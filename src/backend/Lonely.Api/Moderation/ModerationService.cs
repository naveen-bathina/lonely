namespace Lonely.Api.Moderation;

public record SubmitReportRequest(string ReporterId, string TargetId, string Reason, string? ContentId);

public record ReportResponse(
    string ReportId,
    string ReporterId,
    string TargetId,
    string Reason,
    string? ContentId,
    string Status);

public interface IModerationService
{
    Task<ReportResponse> SubmitReport(SubmitReportRequest request);
    Task<IEnumerable<ReportResponse>> GetQueue();
    Task<ReportResponse> Resolve(string reportId, string resolution);
    Task<IEnumerable<ReportResponse>> GetHistory(string userId);
}

public class ModerationService : IModerationService
{
    private readonly Dictionary<string, ReportResponse> _reports = new();

    public Task<ReportResponse> SubmitReport(SubmitReportRequest request)
    {
        var report = new ReportResponse(
            Guid.NewGuid().ToString(),
            request.ReporterId,
            request.TargetId,
            request.Reason,
            request.ContentId,
            "pending");
        _reports[report.ReportId] = report;
        return Task.FromResult(report);
    }

    public Task<IEnumerable<ReportResponse>> GetQueue() =>
        Task.FromResult(_reports.Values.Where(r => r.Status == "pending"));

    public Task<ReportResponse> Resolve(string reportId, string resolution)
    {
        if (!_reports.TryGetValue(reportId, out var report))
            throw new KeyNotFoundException($"Report {reportId} not found.");
        var updated = report with { Status = resolution };
        _reports[reportId] = updated;
        return Task.FromResult(updated);
    }

    public Task<IEnumerable<ReportResponse>> GetHistory(string userId) =>
        Task.FromResult(_reports.Values.Where(r => r.ReporterId == userId));
}
