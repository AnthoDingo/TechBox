using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace TechBox.Helpers
{
    internal class ArgsToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var args = (AutoSuggestBoxQuerySubmittedEventArgs)value;
            return (string)args.QueryText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
