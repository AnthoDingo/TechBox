using System.Windows;
using Wpf.Ui.Controls;

namespace TechBox.SamplePlugin
{
    /// <summary>
    /// Minimal page used as a template for plugin authors, registered by <see cref="SamplePlugin"/>.
    /// </summary>
    public partial class SamplePage : Page, INavigableView<SampleViewModel>
    {
        public SampleViewModel ViewModel { get; }

        public SamplePage(SampleViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void CounterButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Increment();
            CounterButton.Content = $"Cliqué {ViewModel.ClickCount} fois";
        }
    }
}
