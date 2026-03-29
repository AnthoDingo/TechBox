using System.Diagnostics;
using TechBox.Helpers;
using TechBox.ViewModels.Pages.Computers;
using Wpf.Ui.Controls;

namespace TechBox.Views.Pages.Computers
{
    public partial class InfoPage : INavigableView<InfoViewModel>
    {

        public InfoViewModel ViewModel { get; }

        public InfoPage(InfoViewModel viewModel)
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
