using System.Diagnostics;
using System.Windows.Input;
using TechBox.Models.ActiveDirectory;
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

        private void MemberOfListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement { DataContext: Group group })
                ViewModel.ShowGroupMembers(group);
        }
    }
}
