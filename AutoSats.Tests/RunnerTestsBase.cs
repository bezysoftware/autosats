using AutoSats.Configuration;
using AutoSats.Data;
using AutoSats.Execution;
using AutoSats.Execution.Services;
using ExchangeSharp;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace AutoSats.Tests
{
    public abstract class RunnerTestsBase
    {
        protected const string Exchange = "exchange";

        protected readonly List<ExchangeOptions> options;
        protected readonly Mock<IWalletService> wallet;
        protected readonly Mock<IExchangeAPIProvider> apiProvider;
        protected readonly Mock<IExchangeAPI> api;
        protected readonly ExchangeService service;
        protected readonly ExchangeScheduleRunner runner;
        protected readonly SatsContext db;

        public RunnerTestsBase()
        {
            // db
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var opts = new DbContextOptionsBuilder<SatsContext>().UseSqlite(connection).Options;
            this.db = new SatsContext(opts);
            this.db.Database.EnsureCreated();

            // api
            this.api = new Mock<IExchangeAPI>();
            this.apiProvider = new Mock<IExchangeAPIProvider>();
            this.apiProvider.Setup(x => x.GetApi(Exchange)).Returns(() => this.api.Object);

            // service
            this.options = new List<ExchangeOptions>();
            this.wallet = new Mock<IWalletService>();
            this.service = new ExchangeService(Mock.Of<ILogger<ExchangeService>>(), this.apiProvider.Object);
            this.runner = new ExchangeScheduleRunner(
                this.db,
                Mock.Of<ILogger<ExchangeScheduleRunner>>(),
                this.service,
                this.wallet.Object,
                this.options);
        }
    }
}
