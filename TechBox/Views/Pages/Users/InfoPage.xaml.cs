using System.Diagnostics;
using TechBox.ViewModels.Pages.Users;
using Wpf.Ui.Controls;

namespace TechBox.Views.Pages.Users
{
    /// <summary>
    /// Interaction logic for UserInfoPage.xaml
    /// </summary>
    public partial class InfoPage : INavigableView<InfoViewModel>
    {
        public InfoViewModel ViewModel { get; }

        public InfoPage(InfoViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            //Debug.WriteLine($"Looking for : {args.QueryText}");
            //ViewModel.SearchUsers(args.QueryText);
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            Debug.WriteLine($"Request for : {args.SelectedItem.ToString()}");
            ViewModel.GetUser(args.SelectedItem.ToString());
        }
    }
}
