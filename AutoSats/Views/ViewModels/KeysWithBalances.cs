using AutoSats.Models;
using System.Collections.Generic;

namespace AutoSats.Views.ViewModels;

public record KeysWithBalances(ExchangeKeys Keys, IEnumerable<SymbolBalance> Balances)
{
}
