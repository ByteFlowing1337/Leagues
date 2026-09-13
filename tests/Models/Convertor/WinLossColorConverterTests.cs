using System.Globalization;
using System.Windows.Media;
using Leagues.Models.Convertor;

namespace Leagues.Tests.Models.Convertor;

public class WinLossColorConverterTests
{
    private readonly WinLossColorConverter converter = new();

    [Theory]
    [InlineData(true, "Green")]
    [InlineData(false, "Red")]
    public void Convert_ReturnsResultColorForBoolean(bool value, string expectedColor)
    {
        var result = converter.Convert(value, typeof(Brush), null, CultureInfo.InvariantCulture);
        var expected = expectedColor == "Green" ? Brushes.Green : Brushes.Red;

        Assert.Same(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("true")]
    [InlineData(1)]
    public void Convert_ReturnsGrayForNonBoolean(object? value)
    {
        var result = converter.Convert(value, typeof(Brush), null, CultureInfo.InvariantCulture);

        Assert.Same(Brushes.Gray, result);
    }

    [Fact]
    public void ConvertBack_IsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack(Brushes.Green, typeof(bool), null, CultureInfo.InvariantCulture));
    }
}