namespace AutoSats.Validation
{
    public class RequiredIfNotAttribute : RequiredIfAttribute
    {
        public RequiredIfNotAttribute(string propertyName, object? value) : base(propertyName, value)
        {
            InvertCondition = true;
        }
    }
}
