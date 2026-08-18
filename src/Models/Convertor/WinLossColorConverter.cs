using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Leagues.Models.Convertor;

public class WinLossColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool win)
            return win ? Brushes.Green : Brushes.Red;
        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}