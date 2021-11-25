using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AutoSats.Tests.Attributes;

public class CronTest
{
    [Theory]
    [InlineData("lala", false)]
    [InlineData("0 0 0 ? * 1/4 *", true)]
    [InlineData("", true)]
    public void Cron(string cron, bool valid)
    {
        var model = new Model { Cron = cron };

        var result = Validator.TryValidateProperty(
            model.Cron,
            new ValidationContext(model) { MemberName = nameof(Model.Cron) },
            null
        );

        Assert.Equal(valid, result);
    }
}
