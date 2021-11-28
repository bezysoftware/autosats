using AutoMapper;
using AutoSats.Configuration;
using AutoSats.Exceptions;
using AutoSats.Execution.Services;
using AutoSats.Models;
using ExchangeSharp;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoSats.Execution;

public class ExchangeScheduler : IExchangeScheduler
{
    private readonly ILogger<ExchangeScheduler> logger;
    private readonly SatsContext db;
    private readonly IWalletService walletService;
    private readonly ISchedulerFactory schedulerFactory;
    private readonly IExchangeScheduleRunner runner;
    private readonly IExchangeServiceFactory exchangeFactory;
    private readonly IMapper mapper;
    private readonly IEnumerable<ExchangeOptions> exchangeOptions;

    public ExchangeScheduler(
        ILogger<ExchangeScheduler> logger,
        SatsContext db,
        IWalletService walletService,
        ISchedulerFactory schedulerFactory,
        IExchangeScheduleRunner runner,
        IExchangeServiceFactory exchangeFactory,
        IMapper mapper,
        IEnumerable<ExchangeOptions> exchangeOptions)
    {
        this.logger = logger;
        this.db = db;
        this.walletService = walletService;
        this.schedulerFactory = schedulerFactory;
        this.runner = runner;
        this.exchangeFactory = exchangeFactory;
        this.mapper = mapper;
        this.exchangeOptions = exchangeOptions;
    }

    public async Task<IEnumerable<SymbolBalance>> GetSymbolBalancesAsync(string exchange, string key1, string key2, string? key3)
    {
        var service = await this.exchangeFactory.GetServiceAsync(exchange, key1, key2, key3);
        var balances = await service.GetBalancesAsync();
        var options = GetExchangeOptions(exchange);
        var symbols = await service.GetSymbolsWithAsync(options.BitcoinSymbol, options.TickerPrefixes);

        return symbols
            .LeftJoin(balances, x => x.Spend.ToUpper(), x => x.Currency.ToUpper(), (symbol, balance) => new SymbolBalance(symbol, balance?.Amount ?? 0))
            .OrderByDescending(x => x.Amount)
            .ThenBy(x => x.Symbol.Spend)
            .ToArray();
    }

    public async Task<IEnumerable<ExchangeScheduleSummary>> ListSchedulesAsync()
    {
        var schedules = await this.db.ExchangeSchedules
            .Include(x => x.Events.Where(e => e.Type == ExchangeEventType.Buy))
            .AsNoTracking()
            .OrderByDescending(x => x.IsPaused)
            .ToArrayAsync();

        var balancesTasks = schedules
            .Select(x => (x.Id, x.Exchange, Keys: GetKeysFilePath(x)))
            .Select(x => (x.Id, Balances: this.exchangeFactory.GetServiceAsync(x.Exchange, x.Keys).ContinueWith(e => e.Result.GetBalancesAsync()).Unwrap()))
            .ToArray();

        await Task.WhenAll(balancesTasks.Select(x => x.Balances));

        var balances = balancesTasks.ToDictionary(x => x.Id, x => x.Balances.Result);

        return schedules
            .Select(x => this.mapper.Map<ExchangeScheduleSummary>(x) with
            {
                TotalAccumulated = x.Events.Cast<ExchangeEventBuy>().Sum(e => e.Received),
                TotalSpent = x.Events.Count * x.Spend,
                AvailableBTC = balances.GetValueOrDefault(x.Id)?.FirstOrDefault(a => a.Currency == "BTC")?.Amount,
                AvailableSpend = balances.GetValueOrDefault(x.Id)?.FirstOrDefault(a => a.Currency == x.SpendCurrency)?.Amount
            })
            .OrderBy(x => x.NextOccurrence)
            .ToArray();
    }

    public async Task<ExchangeScheduleDetails> GetScheduleDetailsAsync(int id)
    {
        var schedule = await this.db.ExchangeSchedules
            .Include(x => x.Events)
            .Where(x => x.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (schedule == null)
        {
            throw new ScheduleNotFoundException(id);
        }

        var balances = await (await this.exchangeFactory
            .GetServiceAsync(schedule.Exchange, GetKeysFilePath(schedule)))
            .GetBalancesAsync();
        
        var buys = schedule.Events.Where(e => e.Type == ExchangeEventType.Buy).Cast<ExchangeEventBuy>().ToArray();
        var summary = this.mapper.Map<ExchangeScheduleSummary>(schedule) with
        {
            TotalAccumulated = buys.Sum(e => e.Received),
            TotalSpent = buys.Length * schedule.Spend,
            AvailableBTC = balances.FirstOrDefault(a => a.Currency == "BTC")?.Amount,
            AvailableSpend = balances.FirstOrDefault(a => a.Currency == schedule.SpendCurrency)?.Amount
        };

        return new ExchangeScheduleDetails(summary, schedule.Events);
    }

    public async Task DeleteScheduleAsync(int id)
    {
        var schedule = await GetScheduleByIdAsync(id);
        var scheduler = await this.schedulerFactory.GetScheduler();

        await scheduler.UnscheduleJob(GetTriggerKey(id));
        this.db.ExchangeSchedules.Remove(schedule);
        this.db.SaveChanges();
    }

    public async Task PauseScheduleAsync(int id)
    {
        var schedule = await GetScheduleByIdAsync(id);
        var scheduler = await this.schedulerFactory.GetScheduler();

        await scheduler.PauseTrigger(GetTriggerKey(id));

        schedule.IsPaused = true;

        this.db.Add(new ExchangeEventPause
        {
            Schedule = schedule,
            Timestamp = DateTime.UtcNow
        });
        this.db.SaveChanges();
    }

    public async Task ResumeScheduleAsync(int id)
    {
        var schedule = await GetScheduleByIdAsync(id);
        var scheduler = await this.schedulerFactory.GetScheduler();

        await scheduler.ResumeTrigger(GetTriggerKey(id));

        schedule.IsPaused = false;

        this.db.Add(new ExchangeEventResume
        {
            Schedule = schedule,
            Timestamp = DateTime.UtcNow
        });
        this.db.SaveChanges();
    }

    public async Task AddScheduleAsync(NewExchangeSchedule newSchedule, bool runToVerify)
    {
        using var tx = this.db.Database.BeginTransaction();

        var schedule = this.mapper.Map<ExchangeSchedule>(newSchedule);
        var created = new ExchangeEventCreate
        {
            Schedule = schedule,
            Timestamp = DateTime.UtcNow
        };

        this.db.Add(schedule);
        this.db.Add(created);
        this.db.SaveChanges();

        // save keys
        var keysFile = GetKeysFilePath(schedule);
        CryptoUtility.SaveUnprotectedStringsToFile(keysFile, newSchedule.Keys);

        try
        {
            // attempt to run the schedule
            if (runToVerify)
            {
                await this.runner.RunScheduleAsync(schedule.Id);
            }

            // save quartz schedule
            // in case of misfire (AutoSats was down) do nothing
            var scheduler = await this.schedulerFactory.GetScheduler();
            var key = GetTriggerKey(schedule.Id);
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(key)
                .ForJob(ExecutionConsts.ExchangeJobKey)
                .WithCronSchedule(newSchedule.Cron, x => x.WithMisfireHandlingInstructionDoNothing())
                .StartAt(newSchedule.Start)
                .Build();

            // todo: scheduler should use our own transaction
            tx.Commit();

            await scheduler.ScheduleJob(trigger);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Couldn't add new schedule");
            this.db.ChangeTracker.Clear();
            tx.Rollback();
            File.Delete(keysFile);
            throw;
        }
    }

    private async Task<ExchangeSchedule> GetScheduleByIdAsync(int id)
    {
        var schedule = await this.db.ExchangeSchedules.FirstOrDefaultAsync(x => x.Id == id);

        if (schedule == null)
        {
            throw new ScheduleNotFoundException(id);
        }

        return schedule;
    }

    private static TriggerKey GetTriggerKey(int id)
    {
        return new TriggerKey($"{id}");
    }

    private ExchangeOptions GetExchangeOptions(string exchange)
    {
        return this.exchangeOptions.FirstOrDefault(e => e.Name == exchange) ?? new ExchangeOptions();
    }

    private string GetKeysFilePath(ExchangeSchedule x)
    {
        return Path.Combine(this.runner.KeysPath, $"{x.Id}.{ExecutionConsts.KeysFileExtension}");
    }
}
