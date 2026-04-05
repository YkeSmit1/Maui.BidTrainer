using System.Globalization;

namespace Maui.BidTrainer.Converters;

public class CardImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter!.Equals(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = (string)parameter;
        return value!.Equals(true) ? s : s == "bbo" ? "default" : "bbo";
    }
}