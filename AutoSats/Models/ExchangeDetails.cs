namespace AutoSats.Models;

public record ExchangeDetails(string Name, IEnumerable<string> fiatCurrencies)
{
}
