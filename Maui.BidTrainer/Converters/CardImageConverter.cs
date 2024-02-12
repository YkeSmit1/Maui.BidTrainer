using System.Globalization;

namespace Maui.BidTrainer.Converters
{
    public class CardImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter!.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value!.Equals(true) ? parameter : BindableProperty.UnsetValue;
        }
    }
}
