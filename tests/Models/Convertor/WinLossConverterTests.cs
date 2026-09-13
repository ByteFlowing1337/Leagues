using System.Globalization;
using Leagues.Models.Convertor;

namespace Leagues.Tests.Models.Convertor;

public class WinLossConverterTests
{
    private readonly WinLossConverter converter = new();

    [Theory]
    [InlineData(true, "WIN")]
    [InlineData(false, "LOSS")]
    public void Convert_ReturnsResultForBoolean(bool value, string expected)
    {
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("true")]
    [InlineData(1)]
    public void Convert_ReturnsEmptyStringForNonBoolean(object? value)
    {
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ConvertBack_IsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack("WIN", typeof(bool), null, CultureInfo.InvariantCulture));
    }
}
