using AutoSats.Models;

namespace AutoSats.Configuration
{
    public class ExchangeOptions : IExchange
    {
        public string? Name { get; init; }
        
        public string? Key1Name { get; init; }

        public string? Key2Name { get; init; }
        
        public string? Key3Name { get; init; }
        
        public string? ApiUrl { get; init; }
        
        public string? ApiName { get; init; }
        
        public string? Hint { get; init; }
        
        public string[]? Permissions { get; init; }
    }
}
