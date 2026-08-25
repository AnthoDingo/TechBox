using System.Text.RegularExpressions;
using System.Windows.Input;
using TechBox.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace TechBox.Views.Windows
{
    /// <summary>
    /// Modal dialog letting the user choose a power action (shutdown/reboot) and when to run it.
    /// </summary>
    public partial class PowerActionWindow : FluentWindow
    {
        public PowerActionViewModel ViewModel { get; }

        public PowerActionWindow(PowerActionViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            // Setting DisplayDateStart from a XAML binding throws during parsing (WPF DatePicker
            // coercion runs before the control is fully initialized) - assign it once loaded instead.
            Loaded += (_, _) => SchedulePicker.DisplayDateStart = ViewModel.MinimumScheduledDate;
        }

        private void MinutesTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]$");
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
