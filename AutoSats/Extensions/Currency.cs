namespace AutoSats.Extensions
{
    public static class Currency
    {
        public static readonly string CurrencyPairDelimiter = ":";

        public static (string first, string second) Parse(string pair)
        {
            var s = pair.Split(CurrencyPairDelimiter);

            return (s[0], s[1]);
        }

        public static string ToPair(string first, string second)
        {
            return $"{first}{CurrencyPairDelimiter}{second}";
        }
    }
}
