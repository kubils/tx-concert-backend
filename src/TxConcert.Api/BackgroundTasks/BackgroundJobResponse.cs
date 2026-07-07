namespace TxConcert.Api.BackgroundTasks;

public sealed record BackgroundJobResponse(
    Guid JobId,
    string Status);
