using TechBox.Models.ActiveDirectory;
using TechBox.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace TechBox.Views.Windows
{
    /// <summary>
    /// Interaction logic for AdExplorerWindow.xaml
    /// </summary>
    public partial class AdExplorerWindow : FluentWindow
    {
        public AdExplorerViewModel ViewModel { get; }

        public AdExplorerWindow()
        {
            ViewModel = new AdExplorerViewModel();
            DataContext = this;

            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is AdTreeNode node)
                ViewModel.SelectedNode = node;
        }
    }
}
