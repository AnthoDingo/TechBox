using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TechBox.Helpers
{
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value.GetType() != typeof(bool))
            {
                throw new ArgumentException("ExceptionBoolToVisibilityConverterParameterMustBeAnEnumName");
            }

            switch ((bool)value)
            {
                case true:
                    return Visibility.Visible;
                case false:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(Visibility))
            {
                throw new ArgumentException("ExceptionBoolToVisibilityConverterParameterMustBeAnEnumName");
            }

            switch((Visibility)value)
            {
                case Visibility.Visible:
                    return true;
                case Visibility.Collapsed:
                    return false;
                default:
                    return false;
            }
        }
    }
}
