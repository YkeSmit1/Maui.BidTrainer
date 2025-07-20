using System.Globalization;

namespace Maui.BidTrainer.Converters;

public class SymbolColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not string text ? null : Utils.FormatTextWithSymbols(text);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}