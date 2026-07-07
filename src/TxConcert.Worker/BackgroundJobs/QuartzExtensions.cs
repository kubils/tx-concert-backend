using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl.Matchers;

namespace TxConcert.Worker.BackgroundJobs;

public static class QuartzExtensions
{
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string dailyConcertPipelineCron = configuration.GetValue<string>(
            "BackgroundJobs:DailyConcertPipeline:Cron") ?? "0 0 6 * * ?";
        string dailyConcertPipelineTimeZoneId = configuration.GetValue<string>(
            "BackgroundJobs:DailyConcertPipeline:TimeZoneId") ?? "America/Chicago";
        TimeZoneInfo dailyConcertPipelineTimeZone = GetTimeZone(dailyConcertPipelineTimeZoneId);

        services.AddQuartz(q =>
        {
            q.AddJobListener<JobExecutionLogListener>(GroupMatcher<JobKey>.AnyGroup());

            // Command outbox poller — every 5 seconds
            JobKey commandPollerKey = new(CommandOutboxPollerJob.JobKey);
            q.AddJob<CommandOutboxPollerJob>(opts => opts.WithIdentity(commandPollerKey));
            q.AddTrigger(opts => opts
                .ForJob(commandPollerKey)
                .WithIdentity($"{CommandOutboxPollerJob.JobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()));

            // Event outbox poller — every 5 seconds
            JobKey eventPollerKey = new(EventOutboxPollerJob.JobKey);
            q.AddJob<EventOutboxPollerJob>(opts => opts.WithIdentity(eventPollerKey));
            q.AddTrigger(opts => opts
                .ForJob(eventPollerKey)
                .WithIdentity($"{EventOutboxPollerJob.JobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()));

            // Stale command checker — every 1 minute
            JobKey staleKey = new(StaleCommandCheckerJob.JobKey);
            q.AddJob<StaleCommandCheckerJob>(opts => opts.WithIdentity(staleKey));
            q.AddTrigger(opts => opts
                .ForJob(staleKey)
                .WithIdentity($"{StaleCommandCheckerJob.JobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever()));

            // Retention cleanup — every 1 hour
            JobKey retentionKey = new(RetentionJob.JobKey);
            q.AddJob<RetentionJob>(opts => opts.WithIdentity(retentionKey));
            q.AddTrigger(opts => opts
                .ForJob(retentionKey)
                .WithIdentity($"{RetentionJob.JobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever()));

            // Daily concert pipeline — sync 20 concerts, then evaluate them with AI
            JobKey concertPipelineKey = new(DailyConcertPipelineJob.JobKey);
            q.AddJob<DailyConcertPipelineJob>(opts => opts.WithIdentity(concertPipelineKey));
            q.AddTrigger(opts => opts
                .ForJob(concertPipelineKey)
                .WithIdentity($"{DailyConcertPipelineJob.JobKey}-trigger")
                .WithCronSchedule(
                    dailyConcertPipelineCron,
                    x => x.InTimeZone(dailyConcertPipelineTimeZone)));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }

    private static TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException) when (timeZoneId == "America/Chicago")
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        }
    }
}
