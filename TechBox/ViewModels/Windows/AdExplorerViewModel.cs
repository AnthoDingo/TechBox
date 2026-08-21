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
                AdTreeNode node = new AdTreeNode
                {
                    Name = GetProperty(result, "name"),
                    DistinguishedName = GetProperty(result, "distinguishedName"),
                    Description = GetProperty(result, "description"),
                    NodeType = AdNodeType.OrganizationalUnit
                };

                using DirectoryEntry entry = new DirectoryEntry(result.Path);
                LoadChildren(entry, node);
                parentNode.Children.Add(node);
            }
        }

        private static string GetProperty(SearchResult result, string name)
        {
            return result.Properties.Contains(name) && result.Properties[name].Count > 0
                ? result.Properties[name][0]?.ToString() ?? string.Empty
                : string.Empty;
        }
    }
}
