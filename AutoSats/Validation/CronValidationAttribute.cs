using Quartz;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Validation;

public class CronAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var cron = value?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(cron))
        {
            return null;
        }

        var member = validationContext.MemberName ?? string.Empty;

        return CronExpression.IsValidExpression(cron)
            ? ValidationResult.Success
            : new ValidationResult("Value is not a valid cron expression", new[] { member });
    }
}
