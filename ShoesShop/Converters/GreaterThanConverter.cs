using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShoesShop.Converters
{
    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue && parameter is string stringParam)
            {
                if (decimal.TryParse(stringParam, out decimal threshold))
                {
                    return decimalValue > threshold;
                }
            }

            if (value is int intValue && parameter is string stringParam2)
            {
                if (int.TryParse(stringParam2, out int threshold))
                {
                    return intValue > threshold;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}