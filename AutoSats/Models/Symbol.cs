using AutoSats.Extensions;
using System.Linq;

namespace AutoSats.Models;

public record Symbol(string Original, string Accumulate, string Spend)
{
    public readonly static string[] Delimiters = new[] { " ", "-", "_", "/", ":" };

    public static Symbol Normalize(string symbol, string accumulate, char[] prefixes)
    {
        var normalized = Delimiters.Aggregate(symbol.ToUpper(), (acc, d) => acc.Replace(d, ""));

        return new Symbol(symbol, accumulate.TrimStart(1, prefixes), normalized.Replace(accumulate, "").TrimStart(1, prefixes));
    }

    public void Deconstruct(out string accumulate, out string spend)
    {
        accumulate = Accumulate;
        spend = Spend;
    }
}
