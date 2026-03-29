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
        public AdNodeType NodeType { get; set; }
        public ObservableCollection<AdTreeNode> Children { get; set; } = new();

        public string Icon => NodeType switch
        {
            AdNodeType.Root => "🌐",
            AdNodeType.OrganizationalUnit => "📁",
            _ => "📄"
        };
    }
}
