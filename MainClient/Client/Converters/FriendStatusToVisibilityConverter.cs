using Client.MainServer;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Converters
{
    public class FriendStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If there is no friend status then it is collapsed
            if (value == null)
                return Visibility.Collapsed;

            var currentStatus = (FriendStatus)value;
            var neededStatusForVisible = (FriendStatus)Enum.Parse(typeof(FriendStatus), parameter.ToString());

            return currentStatus == neededStatusForVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
