using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data
{
    public class ExchangeSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Exchange { get; set; }

        public string Cron { get; set; }

        [Required]
        public bool IsPaused { get; set; }

        [Required]
        public decimal Spend { get; set; }

        [Required]
        public string CurrencyPair { get; set; }

        [Required]
        public ExchangeWithdrawalType WithdrawalType { get; set; }

        public string WithdrawalAddress { get; set; }

        public decimal WithdrawalAmount { get; set; }

        public virtual ICollection<ExchangeEvent> Events { get; set; }
    }
}
