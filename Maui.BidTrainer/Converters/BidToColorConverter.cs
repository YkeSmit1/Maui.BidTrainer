using System.Globalization;
using Common;

namespace Maui.BidTrainer.Converters;

public class BidToColorConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if  (value is not Bid bid)
            return null;
        return bid.Suit is Suit.Diamonds or Suit.Hearts ? Colors.Red : null;            
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}