using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Text;
using TechBox.Models.ActiveDirectory;

namespace TechBox.ViewModels.Windows
{
    public partial class AdExplorerViewModel : ObservableObject
    {
        private string _ldapPath;

        [ObservableProperty]
        private ObservableCollection<AdTreeNode> _nodes = new();

        [ObservableProperty]
        private AdTreeNode? _selectedNode;

        [ObservableProperty]
        private bool _isLoading;

        [RelayCommand]
        private async Task LoadTreeAsync(string ldapPath)
        {
            _ldapPath = ldapPath;
            IsLoading = true;
            Nodes.Clear();

            try
            {
                AdTreeNode rootNode = await Task.Run(() => BuildTree(ldapPath));
                Nodes.Add(rootNode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while loading  AD : {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }

        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadTreeAsync(_ldapPath);
        }

        private AdTreeNode BuildTree(string ldapPath)
        {
            using DirectoryEntry root = new DirectoryEntry(ldapPath);

            AdTreeNode rootNode = new AdTreeNode
            {
                Name = root.Name,
                DistinguishedName = root.Properties["distinguishedName"].Value?.ToString() ?? string.Empty,
                NodeType = AdNodeType.Root
            };

            LoadChildren(root, rootNode);
            return rootNode;
        }

        private void LoadChildren(DirectoryEntry parent, AdTreeNode parentNode)
        {
            using DirectorySearcher searcher = new DirectorySearcher(parent)
            {
                Filter = "(objectClass=organizationalUnit)",
                SearchScope = SearchScope.OneLevel,
                CacheResults = false
            };

            searcher.PropertiesToLoad.AddRange(new string[]
            {
            "name",
            "distinguishedName",
            "description"
            });

            using SearchResultCollection results = searcher.FindAll();

            foreach (SearchResult result in results)
            {
                using DirectoryEntry entry = result.GetDirectoryEntry();

                AdTreeNode node = new AdTreeNode
                {
                    Name = entry.Properties["name"].Value?.ToString() ?? string.Empty,
                    DistinguishedName = entry.Properties["distinguishedName"].Value?.ToString() ?? string.Empty,
                    Description = entry.Properties["description"].Value?.ToString() ?? string.Empty,
                    NodeType = AdNodeType.OrganizationalUnit
                };

                LoadChildren(entry, node);
                parentNode.Children.Add(node);
            }
        }
    }
}
