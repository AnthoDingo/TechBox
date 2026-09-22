using System.Diagnostics;
using TechBox.ViewModels.Pages.Computers;
using Wpf.Ui.Controls;

namespace TechBox.Views.Pages.Computers
{
    /// <summary>
    /// Interaction logic for BackupProfilePage.xaml
    /// </summary>
    public partial class BackupProfilePage : INavigableView<BackupProfileViewModel>
    {
        public BackupProfileViewModel ViewModel { get; }

        public BackupProfilePage(BackupProfileViewModel viewModel)
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
