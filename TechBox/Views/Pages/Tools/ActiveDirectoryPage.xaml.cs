using TechBox.ViewModels.Pages.Tools;
using Wpf.Ui.Controls;

namespace TechBox.Views.Pages.Tools
{
    public partial class ActiveDirectoryPage : INavigableView<ActiveDirectoryViewModel>
    {
        public ActiveDirectoryViewModel ViewModel { get; }

        public ActiveDirectoryPage(ActiveDirectoryViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
