using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data
{
    public class ExchangeEventBuy : ExchangeEvent
    {
        public string OrderId { get; set; }
     
        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal Received { get; set; }
    }
}
