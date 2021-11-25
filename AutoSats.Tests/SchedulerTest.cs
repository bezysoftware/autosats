using AutoMapper;
using AutoSats.Configuration;
using AutoSats.Data;
using AutoSats.Execution;
using AutoSats.Execution.Services;
using AutoSats.Models;
using ExchangeSharp;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AutoSats.Tests;

public class SchedulerTest : IDisposable
{
    private readonly IWalletService wallet;
    private readonly ILogger<ExchangeScheduler> logger;
    private readonly Mock<ISchedulerFactory> schedulerFactory;
    private readonly Mock<IScheduler> scheduler;
    private readonly Mapper mapper;
    private readonly SatsContext db;
    private readonly Symbol symbol;
    private readonly Mock<IExchangeAPIProvider> apiProvider;
    private readonly ExchangeService service;
    private readonly Mock<IExchangeServiceFactory> serviceProvider;
    private readonly Mock<IExchangeScheduleRunner> runner;
    private readonly ExchangeScheduler escheduler;

    public SchedulerTest()
    {
        this.wallet = Mock.Of<IWalletService>();
        this.logger = Mock.Of<ILogger<ExchangeScheduler>>();
        this.mapper = new Mapper(new MapperConfiguration(x => x.AddProfile<MappingProfile>()));

        // scheduler
        this.schedulerFactory = new Mock<ISchedulerFactory>();
        this.scheduler = new Mock<IScheduler>();
        this.schedulerFactory.Setup(x => x.GetScheduler(It.IsAny<CancellationToken>())).ReturnsAsync(() => this.scheduler.Object);

        // db
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var opts = new DbContextOptionsBuilder<SatsContext>().UseSqlite(connection).Options;
        this.db = new SatsContext(opts);
        this.db.Database.EnsureCreated();

        // default symbol
        this.symbol = new Symbol("BTCUSD", "BTC", "USD");

        // collection
        this.apiProvider = new Mock<IExchangeAPIProvider>();
        this.service = new ExchangeService(Mock.Of<ILogger<ExchangeService>>(), this.apiProvider.Object);
        this.serviceProvider = new Mock<IExchangeServiceFactory>();
        this.serviceProvider
            .Setup(x => x.GetServiceAsync("exchange", "key1", "key2", null))
            .Returns((string exchange, string key1, string key2, string key3) => this.service.InitializeAsync(exchange, key1, key2, key3));

        this.runner = new Mock<IExchangeScheduleRunner>();
        this.escheduler = new ExchangeScheduler(
            this.logger,
            this.db,
            this.wallet,
            this.schedulerFactory.Object,
            this.runner.Object,
            this.serviceProvider.Object,
            this.mapper,
            Array.Empty<ExchangeOptions>());
    }

    public void Dispose()
    {
        this.db.Database.GetDbConnection().Dispose();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ScheduleRunToVerifyCheck(bool runToVerify)
    {
        // running the schedule fails
        this.runner.Setup(x => x.RunScheduleAsync(It.IsAny<int>())).ThrowsAsync(new Exception());
        this.runner.SetupGet(x => x.KeysPath).Returns(Directory.GetCurrentDirectory());

        var request = new NewExchangeSchedule(
            new SymbolBalance(this.symbol, 1),
            "Any",
            Array.Empty<string>(),
            "0 0 0 ? * 1/4 *",
            DateTime.Now,
            ExchangeWithdrawalType.None,
            null,
            null);

        Func<Task> action = () => this.escheduler.AddScheduleAsync(request, runToVerify);

        if (runToVerify)
        {
            // exception is propagated
            await Assert.ThrowsAsync<Exception>(action);
        }
        else
        {
            await action();
        }

        // either nothing or 1 item inserted to schedules
        var expectedCount = runToVerify ? 0 : 1;
        this.db.ExchangeSchedules.ToList().Should().HaveCount(expectedCount);

        // either schedule it with Quartz or not
        var times = runToVerify ? Times.Never() : Times.Once();
        this.scheduler.Verify(x => x.ScheduleJob(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()), times);
    }

    [Fact]
    public async Task GetSymbolBalancesAsyncTest()
    {
        var api = new Mock<IExchangeAPI>();

        api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
        {
            ["BTC"] = 1m,
            ["ETH"] = 2m,
            ["USD"] = 3m,
        });

        api.Setup(x => x.GetMarketSymbolsAsync()).ReturnsAsync(new[] { "BTC_USD", "BTC_EUR", "ETH_BTC", "MKR_BTC", "LTC_EUR", "CZK_BTC" });


        this.apiProvider.Setup(x => x.GetApiAsync("exchange")).ReturnsAsync(() => api.Object);

        var result = await this.escheduler.GetSymbolBalancesAsync("exchange", "key1", "key2", null);

        result.Should().BeEquivalentTo(new[]
        {
                new SymbolBalance(new Symbol("BTC_USD", "BTC", "USD"), 3),
                new SymbolBalance(new Symbol("BTC_EUR", "BTC", "EUR"), 0),
                new SymbolBalance(new Symbol("ETH_BTC", "BTC", "ETH"), 2),
                new SymbolBalance(new Symbol("MKR_BTC", "BTC", "MKR"), 0),
                new SymbolBalance(new Symbol("CZK_BTC", "BTC", "CZK"), 0),
            });
    }
}
