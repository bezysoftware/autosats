using AutoSats.Models;

namespace AutoSats.Views.ViewModels;

public record KeysWithBalances(ExchangeKeys Keys, IEnumerable<SymbolBalance> Balances)
{
}
