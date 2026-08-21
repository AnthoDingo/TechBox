using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TechBox.Models.ActiveDirectory
{
    public enum AdNodeType { Root, OrganizationalUnit }

    public class AdTreeNode
    {
        public string Name { get; set; } = string.Empty;
        public string DistinguishedName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>ADsPath used to bind to this object (e.g. "LDAP://OU=Sales,DC=contoso,DC=com").</summary>
        public string Path { get; set; } = string.Empty;
        public AdNodeType NodeType { get; set; }

        /// <summary>Whether this node has at least one child OU - used to show/hide the expander before children are loaded.</summary>
        public bool HasChildren { get; set; }

        /// <summary>Set once the real children have been fetched, so re-expanding does not re-query the directory.</summary>
        public bool ChildrenLoaded { get; set; }

        /// <summary>Dummy node used only so the TreeView shows an expander before children are actually loaded.</summary>
        public bool IsPlaceholder { get; set; }

        public ObservableCollection<AdTreeNode> Children { get; set; } = new();

        public string Icon => NodeType switch
        {
            AdNodeType.Root => "🌐",
            AdNodeType.OrganizationalUnit => "📁",
            _ => "📄"
        };
    }
}
