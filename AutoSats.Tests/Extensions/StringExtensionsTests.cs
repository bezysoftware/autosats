using AutoSats.Extensions;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace AutoSats.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("XXBT", 1, "XZ", "XBT")]
        [InlineData("XXBT", 2, "XZ", "BT")]
        [InlineData("XXBT", 1, "B", "XXBT")]
        [InlineData("XXBT", 3, "XB", "T")]
        [InlineData("XXBT", 2, "XB", "BT")]
        public void TrimStartLengthTest(string original, int count, string chars, string expected)
        {
            var result = original.TrimStart(count, chars.ToArray());

            result.Should().BeEquivalentTo(expected);
        }
    }
}
