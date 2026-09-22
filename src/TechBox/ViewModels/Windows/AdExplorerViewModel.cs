using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using TechBox.Models.ActiveDirectory;

namespace TechBox.ViewModels.Windows
{
    public partial class AdExplorerViewModel : ObservableObject
    {
        private string _ldapPath = string.Empty;

        [ObservableProperty]
        private ObservableCollection<AdTreeNode> _nodes = new();

        [ObservableProperty]
        private AdTreeNode? _selectedNode;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasSelection;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        partial void OnSelectedNodeChanged(AdTreeNode? value)
        {
            HasSelection = value is not null;
        }

        partial void OnErrorMessageChanged(string value)
        {
            HasError = !string.IsNullOrWhiteSpace(value);
        }

        [RelayCommand]
        private async Task LoadTreeAsync(string ldapPath)
        {
            _ldapPath = ldapPath;
            IsLoading = true;
            ErrorMessage = string.Empty;
            Nodes.Clear();

            try
            {
                AdTreeNode rootNode = await Task.Run(() => BuildRootNode(ldapPath));
                Nodes.Add(rootNode);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Impossible de contacter l'annuaire LDAP du domaine. Vérifiez que ce poste est bien joint à un domaine et réessayez. ({ex.Message})";
                Debug.WriteLine($"Error while loading AD : {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (string.IsNullOrWhiteSpace(_ldapPath))
                return;

            await LoadTreeAsync(_ldapPath);
        }

        [RelayCommand]
        private async Task LoadChildrenAsync(AdTreeNode node)
        {
            if (node is null || node.ChildrenLoaded || !node.HasChildren)
                return;

            try
            {
                List<AdTreeNode> children = await Task.Run(() => GetChildOrganizationalUnits(node.Path));

                node.Children.Clear();
                foreach (AdTreeNode child in children)
                {
                    node.Children.Add(child);
                }

                node.ChildrenLoaded = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Impossible de charger le contenu de « {node.Name} ». ({ex.Message})";
                Debug.WriteLine($"Error while loading children of {node.DistinguishedName} : {ex}");
            }
        }

        private AdTreeNode BuildRootNode(string ldapPath)
        {
            using DirectoryEntry root = new DirectoryEntry(ldapPath);

            AdTreeNode rootNode = new AdTreeNode
            {
                Name = root.Name,
                DistinguishedName = root.Properties["distinguishedName"].Value?.ToString() ?? string.Empty,
                Path = root.Path,
                NodeType = AdNodeType.Root,
                HasChildren = HasChildOrganizationalUnit(root)
            };

            if (rootNode.HasChildren)
            {
                rootNode.Children.Add(CreatePlaceholder());
            }

            return rootNode;
        }

        private List<AdTreeNode> GetChildOrganizationalUnits(string parentPath)
        {
            using DirectoryEntry parent = new DirectoryEntry(parentPath);

            List<AdTreeNode> children = new List<AdTreeNode>();

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
                using DirectoryEntry entry = new DirectoryEntry(result.Path);

                children.Add(new AdTreeNode
                {
                    Name = GetProperty(result, "name"),
                    DistinguishedName = GetProperty(result, "distinguishedName"),
                    Description = GetProperty(result, "description"),
                    Path = result.Path,
                    NodeType = AdNodeType.OrganizationalUnit,
                    HasChildren = HasChildOrganizationalUnit(entry)
                });
            }

            children.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            foreach (AdTreeNode child in children.Where(c => c.HasChildren))
            {
                child.Children.Add(CreatePlaceholder());
            }

            return children;
        }

        private static bool HasChildOrganizationalUnit(DirectoryEntry parent)
        {
            using DirectorySearcher searcher = new DirectorySearcher(parent)
            {
                Filter = "(objectClass=organizationalUnit)",
                SearchScope = SearchScope.OneLevel,
                CacheResults = false
            };
            searcher.PropertiesToLoad.Add("name");

            return searcher.FindOne() is not null;
        }

        private static AdTreeNode CreatePlaceholder() => new AdTreeNode
        {
            Name = "Chargement...",
            NodeType = AdNodeType.OrganizationalUnit,
            IsPlaceholder = true
        };

        private static string GetProperty(SearchResult result, string name)
        {
            return result.Properties.Contains(name) && result.Properties[name].Count > 0
                ? result.Properties[name][0]?.ToString() ?? string.Empty
                : string.Empty;
        }
    }
}
