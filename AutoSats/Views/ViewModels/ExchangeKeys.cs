using AutoSats.Configuration;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Views.ViewModels
{
    public class ExchangeKeys
    {
        [Required]
        public ExchangeOptions? Exchange { get; set; }

        [Required]
        public string? Key1 { get; set; }

        [Required]
        public string? Key2 { get; set; }

        public string? Key3 { get; set; }

        public void Clear()
        {
            Key1 = null;
            Key2 = null;
            Key3 = null;
        }
    }
}
