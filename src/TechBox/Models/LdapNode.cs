using System.Collections.ObjectModel;

namespace TechBox.Models;

/// <summary>
/// A node in the LDAP tree shown by <see cref="Views.LdapPathSelectionWindow"/>.
/// Children are loaded lazily: a node with children starts with a single placeholder
/// child so the TreeView shows an expander, and the placeholder is replaced with the
/// real children the first time the node is expanded.
/// </summary>
public sealed class LdapNode
{
    public LdapNode(string name, string distinguishedName, string path, bool hasChildren)
    {
        Name = name;
        DistinguishedName = distinguishedName;
        Path = path;
        HasChildren = hasChildren;

        if (hasChildren)
        {
            Children.Add(CreatePlaceholder());
        }
    }

    public string Name { get; }

    public string DistinguishedName { get; }

    /// <summary>ADsPath used to bind to this object (e.g. "LDAP://OU=Sales,DC=contoso,DC=com").</summary>
    public string Path { get; }

    public bool HasChildren { get; }

    public bool IsPlaceholder { get; private init; }

    /// <summary>Set once the real children have been fetched, so re-expanding does not re-query the directory.</summary>
    public bool ChildrenLoaded { get; set; }

    public ObservableCollection<LdapNode> Children { get; } = new();

    private static LdapNode CreatePlaceholder() =>
        new("Chargement...", string.Empty, string.Empty, hasChildren: false) { IsPlaceholder = true };
}
