using AutoSats.Data;
using ExchangeSharp;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoSats.Tests
{
    public class RunnerWithdrawalTests : RunnerTestsBase
    {
        [Fact]
        public async Task WithdrawFailsOnSmallBalance()
        {
            AddSchedule(5, "EUR", "BTCEUR", ExchangeWithdrawalType.Fixed, 2, "address");

            this.api.Setup(x => x.GetTickerAsync("BTCEUR")).ReturnsAsync(new ExchangeTicker { Last = 30000m });
            this.api.Setup(x => x.PlaceOrderAsync(It.IsAny<ExchangeOrderRequest>())).ReturnsAsync(new ExchangeOrderResult { AveragePrice = 30000m });
            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["EUR"] = 6,
                ["BTC"] = 1
            });

            await this.runner.RunScheduleAsync(1);

            // only buy event
            var events = this.db.ExchangeEvents.ToList();
            events.Should().HaveCount(1);
            events[0].Type.Should().Be(ExchangeEventType.Buy);

            // withdraw never called
            this.api.Verify(x => x.WithdrawAsync(It.IsAny<ExchangeWithdrawalRequest>()), Times.Never());
        }

        [Fact]
        public async Task WithdrawSucceedsToStaticAddress()
        {
            AddSchedule(5, "EUR", "BTCEUR", ExchangeWithdrawalType.Fixed, 1, "address");

            this.api.Setup(x => x.WithdrawAsync(It.IsAny<ExchangeWithdrawalRequest>())).ReturnsAsync(new ExchangeWithdrawalResponse { Id = "123" });
            this.api.Setup(x => x.GetTickerAsync("BTCEUR")).ReturnsAsync(new ExchangeTicker { Last = 30000m });
            this.api.Setup(x => x.PlaceOrderAsync(It.IsAny<ExchangeOrderRequest>())).ReturnsAsync(new ExchangeOrderResult { AveragePrice = 30000m });
            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["EUR"] = 6,
                ["BTC"] = 1.5m
            });

            await this.runner.RunScheduleAsync(1);

            // buy + withdraw events
            var events = this.db.ExchangeEvents.ToList();
            events.Should().HaveCount(2);
            events[0].Type.Should().Be(ExchangeEventType.Buy);

            var e = events[1] as ExchangeEventWithdrawal;
            e.Type.Should().Be(ExchangeEventType.Withdraw);
            e.Address.Should().Be("address");
            e.Amount.Should().Be(1.5m - Execution.ExecutionConsts.FeeReserve);

            // withdraw called
            var request = new ExchangeWithdrawalRequest
            {
                Address = "address",
                Amount = 1.5m - Execution.ExecutionConsts.FeeReserve,
                Currency = "BTC",
                TakeFeeFromAmount = false
            };
            this.api.Verify(x => x.WithdrawAsync(It.Is<ExchangeWithdrawalRequest>(x => Verify(request, x))), Times.Once());
        }

        [Fact]
        public async Task WithdrawSucceedsToNamedAddress()
        {
            AddSchedule(5, "EUR", "BTCEUR", ExchangeWithdrawalType.Named, 1, "address");

            this.api.Setup(x => x.WithdrawAsync(It.IsAny<ExchangeWithdrawalRequest>())).ReturnsAsync(new ExchangeWithdrawalResponse { Id = "123" });
            this.api.Setup(x => x.GetTickerAsync("BTCEUR")).ReturnsAsync(new ExchangeTicker { Last = 30000m });
            this.api.Setup(x => x.PlaceOrderAsync(It.IsAny<ExchangeOrderRequest>())).ReturnsAsync(new ExchangeOrderResult { AveragePrice = 30000m });
            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["EUR"] = 6,
                ["BTC"] = 1.5m
            });

            await this.runner.RunScheduleAsync(1);

            // withdraw called
            var request = new ExchangeWithdrawalRequest
            {
                Address = null,
                AddressTag = "AutoSats",
                Amount = 1.5m - Execution.ExecutionConsts.FeeReserve,
                Currency = "BTC",
                TakeFeeFromAmount = false
            };
            this.api.Verify(x => x.WithdrawAsync(It.Is<ExchangeWithdrawalRequest>(x => Verify(request, x))), Times.Once());
        }

        [Fact]
        public async Task WithdrawSucceedsToDynamicAddress()
        {
            AddSchedule(5, "EUR", "BTCEUR", ExchangeWithdrawalType.Dynamic, 1, null);

            this.wallet.Setup(x => x.GenerateDepositAddressAsync()).ReturnsAsync("dynamicaddress");
            this.api.Setup(x => x.WithdrawAsync(It.IsAny<ExchangeWithdrawalRequest>())).ReturnsAsync(new ExchangeWithdrawalResponse { Id = "123" });
            this.api.Setup(x => x.GetTickerAsync("BTCEUR")).ReturnsAsync(new ExchangeTicker { Last = 30000m });
            this.api.Setup(x => x.PlaceOrderAsync(It.IsAny<ExchangeOrderRequest>())).ReturnsAsync(new ExchangeOrderResult { AveragePrice = 30000m });
            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["EUR"] = 6,
                ["BTC"] = 1.5m
            });

            await this.runner.RunScheduleAsync(1);

            // buy + withdraw events
            var events = this.db.ExchangeEvents.ToList();
            events.Should().HaveCount(2);
            events[0].Type.Should().Be(ExchangeEventType.Buy);

            var e = events[1] as ExchangeEventWithdrawal;
            e.Type.Should().Be(ExchangeEventType.Withdraw);
            e.Address.Should().Be("dynamicaddress");
            e.Amount.Should().Be(1.5m - Execution.ExecutionConsts.FeeReserve);

            // withdraw called
            var request = new ExchangeWithdrawalRequest
            {
                Address = "dynamicaddress",
                Amount = 1.5m - Execution.ExecutionConsts.FeeReserve,
                Currency = "BTC",
                TakeFeeFromAmount = false
            };
            this.api.Verify(x => x.WithdrawAsync(It.Is<ExchangeWithdrawalRequest>(x => Verify(request, x))), Times.Once());
        }
    }
}
