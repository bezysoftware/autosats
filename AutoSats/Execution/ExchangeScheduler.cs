using AutoMapper;
using AutoSats.Data;
using AutoSats.Exceptions;
using AutoSats.Execution.Services;
using AutoSats.Models;
using ExchangeSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSats.Execution
{
    public class ExchangeScheduler : IExchangeScheduler
    {
        private readonly ILogger<ExchangeScheduler> logger;
        private readonly SatsContext db;
        private readonly IWalletService walletService;
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IExchangeScheduleRunner runner;
        private readonly IMapper mapper;

        public ExchangeScheduler(
            ILogger<ExchangeScheduler> logger, 
            SatsContext db,
            IWalletService walletService,
            ISchedulerFactory schedulerFactory,
            IExchangeScheduleRunner runner,
            IMapper mapper)
        {
            this.logger = logger;
            this.db = db;
            this.walletService = walletService;
            this.schedulerFactory = schedulerFactory;
            this.runner = runner;
            this.mapper = mapper;
        }

        public Task<string> GenerateNewAddressAsync()
        {
            return this.walletService.GenerateDepositAddressAsync();
        }

        public async Task<IEnumerable<ExchangeSchedule>> ListSchedulesAsync()
        {
            return await this.db.ExchangeSchedules
                .AsNoTracking()
                .OrderByDescending(x => x.IsPaused)
                .ToArrayAsync();
        }

        public async Task<ExchangeSchedule> GetScheduleDetailAsync(int id)
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

            return schedule;
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
            this.db.SaveChanges();
        }

        public async Task ResumeScheduleAsync(int id)
        {
            var schedule = await GetScheduleByIdAsync(id);
            var scheduler = await this.schedulerFactory.GetScheduler();

            await scheduler.ResumeTrigger(GetTriggerKey(id));

            schedule.IsPaused = false;
            this.db.SaveChanges();
        }

        public async Task AddScheduleAsync(NewExchangeSchedule newSchedule)
        {
            using var tx = this.db.Database.BeginTransaction();

            var schedule = this.mapper.Map<ExchangeSchedule>(newSchedule);

            this.db.Add(schedule);
            this.db.SaveChanges();

            // save keys
            var keysFile = Path.Combine(this.runner.KeysPath, $"{schedule.Id}.{ExecutionConsts.KeysFileExtension}");
            CryptoUtility.SaveUnprotectedStringsToFile(keysFile, newSchedule.Keys);

            try
            {
                // attempt to run the schedule
                await this.runner.RunScheduleAsync(schedule.Id);

                // save quartz schedule
                var key = GetTriggerKey(schedule.Id);
                var trigger = TriggerBuilder
                    .Create()
                    .WithIdentity(key)
                    .ForJob(ExecutionConsts.ExchangeJobKey)
                    .WithCronSchedule(newSchedule.Cron)
                    .Build();

                var scheduler = await this.schedulerFactory.GetScheduler();
                await scheduler.ScheduleJob(trigger);

                tx.Commit();
            }
            catch(Exception ex)
            {
                this.logger.LogError(ex, "Couldn't add new schedule");
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
    }
}
