using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Client.Converters
{
    public class MessageIdToColorBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var id = (int)value;
            if (id == Globals.LoggedUser.Id)
                return Brushes.Aqua;
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
