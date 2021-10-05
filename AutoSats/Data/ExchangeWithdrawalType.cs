using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data
{
    public enum ExchangeWithdrawalType
    {
        [Display(Name = "No withdrawal")]
        None,

        [Display(Name = "Fixed address")]
        Fixed,

        [Display(Name = "New address each time")]
        Dynamic,

        [Display(Name = "Fixed named address")]
        Named
    }
}
