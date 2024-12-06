using System.Globalization;

namespace Maui.BidTrainer.Converters;

public class BoolToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var correct = (bool)value!;
        return correct ? "correct.png" : "incorrect.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
            throw new NotImplementedException();
    }
}