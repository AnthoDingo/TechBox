using System.Diagnostics;
using TechBox.ViewModels.Pages.Computers;
using Wpf.Ui.Controls;

namespace TechBox.Views.Pages.Computers
{
    public partial class SCCMPage : INavigableView<SCCMViewModel>
    {

        public SCCMViewModel ViewModel { get; }

        public SCCMPage(SCCMViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private async void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            Debug.WriteLine(args);
            await ViewModel.SetComputer(args.SelectedItem.ToString());
        }
    }
}
