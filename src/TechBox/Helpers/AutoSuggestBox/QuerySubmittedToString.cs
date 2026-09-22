using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input.StylusPlugIns;
using Wpf.Ui.Controls;

namespace TechBox.Helpers.AutoSuggestBox
{
    internal class QuerySubmittedToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value.GetType() != typeof(AutoSuggestBoxQuerySubmittedEventArgs))
            {
                throw new ArgumentException("ExceptionQuerySubmittedToString");
            }

            try
            {
                var args = (AutoSuggestBoxQuerySubmittedEventArgs)value;
                return (string)args.QueryText;
            } catch(Exception e) {
                Debug.WriteLine(e); 
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
