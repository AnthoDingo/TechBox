using System;
using System.ComponentModel;
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
        private readonly bool _isMandatory;
        private bool _confirmed;

        public AdExplorerViewModel ViewModel { get; }

        /// <summary>Opens the explorer starting from the domain root, selection optional.</summary>
        public AdExplorerWindow() : this(initialLdapPath: null, isMandatory: false)
        {
        }

        /// <summary>Opens the explorer starting from <paramref name="initialLdapPath"/>, or the domain root when null/empty.</summary>
        public AdExplorerWindow(string? initialLdapPath) : this(initialLdapPath, isMandatory: false)
        {
        }

        /// <summary>
        /// When <paramref name="isMandatory"/> is true, the Cancel button is hidden and closing the
        /// window without a confirmed selection asks whether to exit the whole application instead.
        /// </summary>
        public AdExplorerWindow(string? initialLdapPath, bool isMandatory)
        {
            _isMandatory = isMandatory;

            ViewModel = new AdExplorerViewModel();
            DataContext = this;

            InitializeComponent();

            CancelButton.Visibility = isMandatory ? Visibility.Collapsed : Visibility.Visible;
            Closing += Window_Closing;

            Loaded += async (_, _) =>
            {
                string? ldapPath = initialLdapPath;

                if (string.IsNullOrWhiteSpace(ldapPath))
                {
                    try
                    {
                        ldapPath = GetDomainRootLdapPath();
                    }
                    catch (Exception ex)
                    {
                        ViewModel.ErrorMessage =
                            $"Impossible de déterminer le domaine Active Directory. Vérifiez que ce poste est bien joint à un domaine. ({ex.Message})";
                        return;
                    }
                }

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
            if (e.NewValue is AdTreeNode { IsPlaceholder: false } node)
                ViewModel.SelectedNode = node;
        }

        private async void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not System.Windows.Controls.TreeViewItem { DataContext: AdTreeNode node })
                return;

            if (node.ChildrenLoaded || !node.HasChildren)
                return;

            await ViewModel.LoadChildrenCommand.ExecuteAsync(node);
            e.Handled = true;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.HasSelection)
                return;

            _confirmed = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_confirmed || !_isMandatory)
                return;

            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
                this,
                "La sélection d'un emplacement LDAP est obligatoire pour utiliser TechBox. Voulez-vous quitter l'application ?",
                "Sélection obligatoire",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
                Application.Current.Shutdown();
            else
                e.Cancel = true;
        }
    }
}
