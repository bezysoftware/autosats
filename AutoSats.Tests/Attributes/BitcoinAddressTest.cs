using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AutoSats.Tests.Attributes;

public class BitcoinAddressTest
{
    [Theory]
    [InlineData("bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh")]
    [InlineData("35ih8ZuVQhhgyYmLrXhHLrT6AcSfMNqX1q")]
    [InlineData("1MKmgsj5fKohBAJFRsnGymZ19o1rKXrX63")]
    [InlineData(" 1MKmgsj5fKohBAJFRsnGymZ19o1rKXrX63 ")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidAddress(string address)
    {
        var model = new Model { Address = address };

        Validator.ValidateProperty(model.Address, new ValidationContext(model) { MemberName = nameof(Model.Address) });
    }

    [Theory]
    [InlineData("blabla")]
    [InlineData("1MKmgsj5fKohBAJFRsnGymZ19o1rKXrX64")]
    public void InvalidAddress(string address)
    {
        var model = new Model { Address = address };
        var result = Validator.TryValidateProperty(model.Address, new ValidationContext(model) { MemberName = nameof(Model.Address) }, null);

        Assert.False(result);
    }
}
