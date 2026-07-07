using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;
using NodaTime;

namespace TxConcert.Domain.Jobs;

public sealed class JobExecutionLogEntity : BaseEntity
{
    private JobExecutionLogEntity() { } // EF Core

    public static JobExecutionLogEntity Start(
        string jobName,
        string jobGroup,
        string triggerName,
        string triggerGroup,
        string fireInstanceId,
        Instant startedAt)
    {
        var entity = new JobExecutionLogEntity
        {
            JobName = jobName,
            JobGroup = jobGroup,
            TriggerName = triggerName,
            TriggerGroup = triggerGroup,
            FireInstanceId = fireInstanceId,
            Status = JobExecutionStatus.Running,
            StartedAt = startedAt
        };
        entity.SetId(Constants.IdPrefix.JobExecutionLog);
        return entity;
    }

    public string JobName { get; private set; } = default!;
    public string JobGroup { get; private set; } = default!;
    public string TriggerName { get; private set; } = default!;
    public string TriggerGroup { get; private set; } = default!;
    public string FireInstanceId { get; private set; } = default!;
    public JobExecutionStatus Status { get; private set; }
    public Instant StartedAt { get; private set; }
    public Instant? CompletedAt { get; private set; }
    public long? DurationMs { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void Complete(Instant completedAt)
    {
        Status = JobExecutionStatus.Succeeded;
        CompletedAt = completedAt;
        DurationMs = (long)(completedAt - StartedAt).TotalMilliseconds;
        ErrorMessage = null;
    }

    public void Fail(Instant completedAt, string errorMessage)
    {
        Status = JobExecutionStatus.Failed;
        CompletedAt = completedAt;
        DurationMs = (long)(completedAt - StartedAt).TotalMilliseconds;
        ErrorMessage = errorMessage;
    }
}
