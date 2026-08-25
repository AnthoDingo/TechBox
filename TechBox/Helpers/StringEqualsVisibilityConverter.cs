using System.Globalization;
using System.Windows.Data;

namespace TechBox.Helpers
{
    /// <summary>Visible when the bound value's string representation matches ConverterParameter, Collapsed otherwise.</summary>
    internal class StringEqualsVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.Ordinal)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
