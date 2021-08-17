using AutoSats.Data;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Models
{
    public class NewExchangeSchedule
    {
        [Required]
        public string CurrencyPair { get; set; }

        [Required]
        public string Exchange { get; set; }

        [Required]
        public string[] Keys { get; set; }

        [Required]
        public string Cron { get; set; }

        public decimal Spend { get; set; }

        public string WithdrawalAddress { get; set; }

        public decimal WithdrawalAmount { get; set; }

        public ExchangeWithdrawalType WithdrawalType { get; set; }
    }
}
