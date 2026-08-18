using System.Globalization;
using System.Windows.Data;

namespace Leagues.Models.Convertor;

public class WinLossConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool win)
            return win ? "WIN" : "LOSS";
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}