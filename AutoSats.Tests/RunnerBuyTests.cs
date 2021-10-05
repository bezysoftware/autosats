using AutoSats.Configuration;
using AutoSats.Data;
using AutoSats.Exceptions;
using ExchangeSharp;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoSats.Tests
{
    public class RunnerBuyTests : RunnerTestsBase
    {
        [Fact]
        public async Task BuyFailsOnSmallBalance()
        {
            AddSchedule(5, "EUR", "BTCEUR");

            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["EUR"] = 4
            });

            await Assert.ThrowsAsync<ScheduleRunFailedException>(() => this.runner.RunScheduleAsync(1));

            var events = this.db.ExchangeEvents.ToList();
            events.Should().HaveCount(1);
            events[0].Type.Should().Be(ExchangeEventType.Buy);
            events[0].Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarketBuySucceeds()
        {
            AddSchedule(5, "EUR", "BTCEUR");

            this.api.Setup(x => x.GetTickerAsync("BTCEUR")).ReturnsAsync(new ExchangeTicker { Last = 30000m });
            this.api.Setup(x => x.PlaceOrderAsync(It.IsAny<ExchangeOrderRequest>())).ReturnsAsync(new ExchangeOrderResult { AveragePrice = 30000m });
            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["EUR"] = 6
            });

            await this.runner.RunScheduleAsync(1);

            // added buy event
            var events = this.db.ExchangeEvents.ToList();
            events.Should().HaveCount(1);
            var e = events[0] as ExchangeEventBuy;
            e.Type.Should().Be(ExchangeEventType.Buy);
            e.Error.Should().BeNull();

            // api request
            var request = new ExchangeOrderRequest
            {
                Amount = 5 / 30000m,
                IsBuy = true,
                MarketSymbol = "BTCEUR",
                OrderType = OrderType.Market
            };
            this.api.Verify(x => x.PlaceOrderAsync(It.Is<ExchangeOrderRequest>(x => Verify(request, x))), Times.Once());
        }

        [Fact]
        public async Task LimitBuySucceeds()
        {
            AddSchedule(100, "EUR", "BTCEUR");

            this.options.Add(new ExchangeOptions { Name = Exchange, BuyOrderType = Models.BuyOrderType.Limit });
            this.api.Setup(x => x.GetTickerAsync("BTCEUR")).ReturnsAsync(new ExchangeTicker { Last = 30000m });
            this.api.Setup(x => x.PlaceOrderAsync(It.IsAny<ExchangeOrderRequest>())).ReturnsAsync(new ExchangeOrderResult { AveragePrice = 30000m });
            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["EUR"] = 200
            });

            await this.runner.RunScheduleAsync(1);

            var request = new ExchangeOrderRequest
            {
                Amount = 100 / 30000m,
                IsBuy = true,
                MarketSymbol = "BTCEUR",
                OrderType = OrderType.Limit,
                Price = 30300m
            };
            this.api.Verify(x => x.PlaceOrderAsync(It.Is<ExchangeOrderRequest>(x => Verify(request, x))), Times.Once());
        }

        [Fact]
        public async Task InvertMarketBuySucceeds()
        {
            AddSchedule(5, "LTC", "LTCBTC");

            this.api.Setup(x => x.GetTickerAsync("LTCBTC")).ReturnsAsync(new ExchangeTicker { Last = 0.003m });
            this.api.Setup(x => x.PlaceOrderAsync(It.IsAny<ExchangeOrderRequest>())).ReturnsAsync(new ExchangeOrderResult { AveragePrice = 0.003m });
            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["LTC"] = 6
            });

            await this.runner.RunScheduleAsync(1);

            // api request
            var request = new ExchangeOrderRequest
            {
                Amount = 5,
                IsBuy = false,
                MarketSymbol = "LTCBTC",
                OrderType = OrderType.Market
            };
            this.api.Verify(x => x.PlaceOrderAsync(It.Is<ExchangeOrderRequest>(x => Verify(request, x))), Times.Once());
        }

        [Fact]
        public async Task InvertLimitBuySucceeds()
        {
            AddSchedule(5, "LTC", "LTCBTC");

            this.options.Add(new ExchangeOptions { Name = Exchange, BuyOrderType = Models.BuyOrderType.Limit });
            this.api.Setup(x => x.GetTickerAsync("LTCBTC")).ReturnsAsync(new ExchangeTicker { Last = 0.00300m });
            this.api.Setup(x => x.PlaceOrderAsync(It.IsAny<ExchangeOrderRequest>())).ReturnsAsync(new ExchangeOrderResult { AveragePrice = 0.003m });
            this.api.Setup(x => x.GetAmountsAsync()).ReturnsAsync(new Dictionary<string, decimal>
            {
                ["LTC"] = 6
            });

            await this.runner.RunScheduleAsync(1);

            // api request
            var request = new ExchangeOrderRequest
            {
                Amount = 5,
                Price = 0.00297m,
                IsBuy = false,
                MarketSymbol = "LTCBTC",
                OrderType = OrderType.Limit
            };
            this.api.Verify(x => x.PlaceOrderAsync(It.Is<ExchangeOrderRequest>(x => Verify(request, x))), Times.Once());
        }
    }
}
