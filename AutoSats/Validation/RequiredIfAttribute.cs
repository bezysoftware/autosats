using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AutoSats.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class RequiredIfAttribute : RequiredAttribute
{
    private string propertyName;
    private object? value;

    public RequiredIfAttribute(string propertyName, object? value)
    {
        this.propertyName = propertyName;
        this.value = value;
    }

    public bool InvertCondition { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var instance = context.ObjectInstance;
        var type = instance.GetType();
        var propertyValue = type.GetProperty(this.propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(instance);

        var equals = (propertyValue == null && this.value == null) || (propertyValue?.Equals(this.value) ?? false);

        if (InvertCondition)
        {
            equals = !equals;
        }

        if (!equals)
        {
            return ValidationResult.Success;
        }

        return base.IsValid(value, context);
    }
}
