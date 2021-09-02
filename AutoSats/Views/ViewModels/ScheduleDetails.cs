using AutoSats.Data;
using AutoSats.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Views.ViewModels
{
    public class ScheduleDetails
    {
        [Required]
        [Cron]
        public string Cron { get; set; } = string.Empty;

        [Required]
        public string Currency { get; set; } = string.Empty;

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Value must be greater than zero.")]
        public decimal Spend { get; set; }

        [Required]
        public DateTime Start { get; set; } = DateTime.Now;

        [Required]
        public ExchangeWithdrawalType WithdrawalType { get; set; }

        [BitcoinAddress]
        [RequiredIf(nameof(WithdrawalType), ExchangeWithdrawalType.Fixed, ErrorMessage = "Address is required.")]
        public string? WithdrawalAddress { get; set; }

        [Range(0.0000001, double.MaxValue, ErrorMessage = "Value must be greater than zero.")]
        [RequiredIfNot(nameof(WithdrawalType), ExchangeWithdrawalType.None, ErrorMessage = "Amount is required.")]
        public decimal? WithdrawalLimit { get; set; }

        [Required]
        public bool RunToVerify { get; set; } = true;
    }
}