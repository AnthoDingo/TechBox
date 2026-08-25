using System.Diagnostics;
using TechBox.ViewModels.Pages.Computers;
using Wpf.Ui.Controls;

namespace TechBox.Views.Pages.Computers
{
    public partial class PowerPage : INavigableView<PowerViewModel>
    {

        public PowerViewModel ViewModel { get; }

        public PowerPage(PowerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            Debug.WriteLine(args);
            ViewModel.GetComputer(args.SelectedItem.ToString());
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Debug.WriteLine(args);
        }
    }
}
