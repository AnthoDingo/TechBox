using TechBox.ViewModels.Pages.Tools;
using Wpf.Ui.Controls;

namespace TechBox.Views.Pages.Tools
{
    public partial class PowerShellPage : INavigableView<PowerShellViewModel>
    {
        public PowerShellViewModel ViewModel { get; }

        public PowerShellPage(PowerShellViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
