using AutoSats.Data;

namespace AutoSats.Models
{
    public record NewExchangeSchedule(
        string CurrencyPair,
        string Exchange,
        string[] Keys,
        string Cron,
        decimal Spend,
        ExchangeWithdrawalType WithdrawalType)
    { 
        public string? WithdrawalAddress { get; set; }

        public decimal WithdrawalAmount { get; set; }
    }
}
