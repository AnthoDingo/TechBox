using System.DirectoryServices;
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

        /// <summary>Opens the explorer starting from the domain root.</summary>
        public AdExplorerWindow() : this(initialLdapPath: null)
        {
        }

        /// <summary>Opens the explorer starting from <paramref name="initialLdapPath"/>, or the domain root when null/empty.</summary>
        public AdExplorerWindow(string? initialLdapPath)
        {
            ViewModel = new AdExplorerViewModel();
            DataContext = this;

            InitializeComponent();

            Loaded += async (_, _) =>
            {
                string ldapPath = string.IsNullOrWhiteSpace(initialLdapPath)
                    ? GetDomainRootLdapPath()
                    : initialLdapPath;

                await ViewModel.LoadTreeCommand.ExecuteAsync(ldapPath);
            };
        }

        private static string GetDomainRootLdapPath()
        {
            using DirectoryEntry rootDse = new("LDAP://RootDSE");
            string defaultNamingContext = (string)rootDse.Properties["defaultNamingContext"].Value;
            return $"LDAP://{defaultNamingContext}";
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is AdTreeNode node)
                ViewModel.SelectedNode = node;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
