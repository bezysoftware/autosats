using Quartz;

namespace AutoSats.Execution;

[DisallowConcurrentExecution]
public class ExchangeJob : IJob
{
    private readonly ILogger<ExchangeJob> logger;
    private readonly IExchangeScheduleRunner runner;

    public ExchangeJob(ILogger<ExchangeJob> logger, IExchangeScheduleRunner runner)
    {
        this.logger = logger;
        this.runner = runner;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var triggerName = context.Trigger.Key.Name;
        if (!int.TryParse(triggerName, out var id))
        {
            this.logger.LogError($"Unable to parse '{triggerName}' to int id");
            return;
        }

        this.logger.LogInformation($"Exchange buy job started for schedule id '{id}'");

        try
        {
            await this.runner.RunScheduleAsync(id);

            this.logger.LogInformation($"Exchange buy job completed for schedule id '{id}'");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"Exchange buy job failed for schedule id '{id}'");
        }
    }
}
