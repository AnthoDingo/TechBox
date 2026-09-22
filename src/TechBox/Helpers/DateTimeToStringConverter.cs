using System.Globalization;
using System.Windows.Data;

namespace TechBox.Helpers
{
    class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                DateTime dtUTC = dateTime.ToUniversalTime();
                DateTime baseUTC = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                if (dateTime.ToUniversalTime() == new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    return "Never";

                return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
