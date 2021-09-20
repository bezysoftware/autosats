using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AutoSats.Tests.Attributes
{
    public class RequiredIfTest
    {
        [Theory]
        [InlineData("set", "set", true)]
        [InlineData("set", "", false)]
        [InlineData("", "set", true)]
        [InlineData("", "", true)]
        public void PropertySet(string address, string addressTag, bool success)
        {
            // if address is set then addressTag becomes required, otherwise it isn't
            var model = new Model
            {
                Address = address,
                AddressTag = addressTag
            };

            var result = Validator.TryValidateProperty(
                model.AddressTag,
                new ValidationContext(model) { MemberName = nameof(Model.AddressTag) },
                null
            );

            Assert.Equal(success, result);
        }
    }
}
