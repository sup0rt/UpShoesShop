using System;
using System.Globalization;
using System.Windows.Data;

namespace ShoesShop.Converters
{
    public class HighDiscountCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Простая проверка на nullable decimal
            decimal? discount = value as decimal?;

            if (discount.HasValue)
            {
                return discount.Value >= 15;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}