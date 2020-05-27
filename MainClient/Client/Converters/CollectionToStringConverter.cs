using Client.MainServer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    public class CollectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dict = value as IDictionary<int, User>;
            if (dict == null) return string.Empty;

            var returnedString = "";

            foreach (var item in dict)
            {
                returnedString += item.Value.Name + "\n";
            }

            return returnedString.Remove(returnedString.Length - 1, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
