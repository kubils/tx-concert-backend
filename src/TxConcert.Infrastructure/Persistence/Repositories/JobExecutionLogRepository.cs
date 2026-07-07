using TxConcert.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class JobExecutionLogRepository(ApplicationDbContext context)
    : BaseRepository<JobExecutionLogEntity>(context)
{
    public async Task<JobExecutionLogEntity?> GetRunningByFireInstanceIdAsync(
        string fireInstanceId,
        CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            log => log.FireInstanceId == fireInstanceId && log.Status == JobExecutionStatus.Running,
            ct);
    }
}
