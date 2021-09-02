using AutoSats.Models;
using System;

namespace AutoSats.Configuration
{
    public class ExchangeOptions : IExchange
    {
        public string Name { get; init; } = string.Empty;

        public string Key1Name { get; init; } = string.Empty;

        public string Key2Name { get; init; } = string.Empty;

        public string Key3Name { get; init; } = string.Empty;

        public string ApiUrl { get; init; } = string.Empty;

        public string ApiName { get; init; } = string.Empty;

        public string Hint { get; init; } = string.Empty;

        public string[] Permissions { get; init; } = Array.Empty<string>();
    }
}
