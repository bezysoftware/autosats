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
        public string Key1 { get; set; } = string.Empty;

        [Required]
        public string Key2 { get; set; } = string.Empty;

        [RequiredIfNot(nameof(Key3Name), "")]
        public string Key3 { get; set; } = string.Empty;

        private string Key3Name => Exchange?.Key3Name ?? "";

        public void Clear()
        {
            Key1 = string.Empty;
            Key2 = string.Empty;
            Key3 = string.Empty;
        }

        public string[] ToKeysArray()
        {
            return Key3Name == string.Empty
                ? new[] { Key1, Key2 }
                : new[] { Key1, Key2, Key3 };
        }
    }
}
