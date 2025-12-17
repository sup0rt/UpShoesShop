using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ShoesShop.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageName = value as string;

            if (string.IsNullOrEmpty(imageName))
            {
                return new BitmapImage(new Uri("pack://application:,,,/Photos/picture.png"));
            }

            try
            {
                return new BitmapImage(new Uri($"pack://application:,,,/Photos/{imageName}"));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/Photos/picture.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}