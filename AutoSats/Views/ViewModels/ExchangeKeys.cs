using AutoSats.Configuration;
using AutoSats.Validation;
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

        [RequiredIfNot(nameof(Key3Name), null)]
        public string? Key3 { get; set; }

        private string? Key3Name => Exchange?.Key3Name;

        public void Clear()
        {
            Key1 = null;
            Key2 = null;
            Key3 = null;
        }
    }
}
