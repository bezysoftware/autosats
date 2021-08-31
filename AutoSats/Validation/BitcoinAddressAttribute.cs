using NBitcoin;
using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Validation
{
    public class BitcoinAddressAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var text = value?.ToString()?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            try
            {
                BitcoinAddress.Create(text, Network.Main);
                return ValidationResult.Success;
            }
            catch (FormatException)
            {
                return new ValidationResult("Value doesn't seem to be a valid Bitcoin address", new[] { context.DisplayName });
            }
        }
    }
}
