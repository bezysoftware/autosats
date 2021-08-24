using System.Collections.Generic;

namespace AutoSats.Models
{
    public record ExchangeDetails(IEnumerable<string> fiatCurrencies)
    {        
    }
}
