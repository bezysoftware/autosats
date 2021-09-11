using AutoSats.Models;

namespace AutoSats.Extensions
{
    public static class BusinessExtensions
    {
        public static string GetImgLogoPath(this IExchange exchange) => $"/img/{exchange.Name?.ToLower()}/logo.png";
        public static string GetImgConfigPath(this IExchange exchange) => $"/img/{exchange.Name?.ToLower()}/config.png";
    }
}
