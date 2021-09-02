using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data
{
    public class ExchangeSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Exchange { get; set; } = null!;

        [Required]
        public string Cron { get; set; } = null!;

        [Required]
        public bool IsPaused { get; set; }

        [Required]
        public decimal Spend { get; set; }

        [Required]
        public string CurrencyPair { get; set; } = null!;

        [Required]
        public ExchangeWithdrawalType WithdrawalType { get; set; }

        [Required]
        public DateTime Start { get; set; }

        public string? WithdrawalAddress { get; set; }

        public decimal WithdrawalLimit { get; set; }

        public virtual ICollection<ExchangeEvent> Events { get; set; } = null!;
    }
}
