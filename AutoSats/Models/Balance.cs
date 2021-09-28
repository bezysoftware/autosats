namespace AutoSats.Models
{
    public record Balance(string Currency, decimal Amount)
    {
    }

    public record SymbolBalance(Symbol Symbol, decimal Amount)
    {
    }
}
