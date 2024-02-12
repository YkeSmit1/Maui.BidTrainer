using System.Globalization;

namespace Maui.BidTrainer.Converters
{
    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var correct = (bool)value!;
            if (correct)
                return "correct.png";
            else
                return "incorrect.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
