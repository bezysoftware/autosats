using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data
{
    public class ExchangeEventWithdrawal : ExchangeEvent
    {
        public string? WithdrawalId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string? Address { get; set; }
     
        public override ExchangeEventType Type { get; set; } = ExchangeEventType.Withdraw;
    }
}
