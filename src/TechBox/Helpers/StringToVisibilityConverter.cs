using System.Globalization;
using System.Windows.Data;

namespace TechBox.Helpers
{
    internal class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value.GetType() != typeof(string))
            {
                throw new ArgumentException("ExceptionStringToVisibilityConverterParameterMustBeAnEnumName");
            }

            if (string.IsNullOrEmpty(value.ToString()))
                return Visibility.Collapsed;

            switch (value.ToString().ToLower())
            {
                case "visible":
                    return Visibility.Visible;
                case "collasped":
                    return Visibility.Collapsed;
                case "hidden":
                    return Visibility.Hidden;
                default:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(Visibility))
            {
                throw new ArgumentException("ExceptionStringToVisibilityConverterParameterMustBeAnEnumName");
            }

            switch((Visibility)value)
            {
                case Visibility.Visible:
                    return "visible";
                case Visibility.Collapsed:
                    return "collasped";
                case Visibility.Hidden:
                    return "hidden";
                default:
                    return "unknown";
            }
        }
    }
}
