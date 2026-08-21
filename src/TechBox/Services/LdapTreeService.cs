using System;
using System.Collections.Generic;
using System.DirectoryServices;
using TechBox.Models;

namespace TechBox.Services;

/// <summary>
/// Browses the LDAP directory of the domain the current machine is joined to,
/// exposing only container-like objects (OUs, containers, the domain itself, ...)
/// so the user can pick a location without being distracted by users/computers/groups.
/// </summary>
public sealed class LdapTreeService : ILdapTreeService
{
    private static readonly string[] ContainerClasses =
    {
        "organizationalUnit",
        "container",
        "builtinDomain",
        "domainDNS",
    };

    public LdapNode GetRootNode()
    {
        using var rootDse = new DirectoryEntry("LDAP://RootDSE");
        var defaultNamingContext = (string)rootDse.Properties["defaultNamingContext"].Value;

        using var domainEntry = new DirectoryEntry($"LDAP://{defaultNamingContext}");
        return new LdapNode(
            FormatName(domainEntry),
            defaultNamingContext,
            domainEntry.Path,
            hasChildren: HasContainerChildren(domainEntry));
    }

    public IReadOnlyList<LdapNode> GetChildren(LdapNode node)
    {
        var children = new List<LdapNode>();

        using var entry = new DirectoryEntry(node.Path);
        foreach (DirectoryEntry child in entry.Children)
        {
            using (child)
            {
                if (!IsContainer(child))
                {
                    continue;
                }

                children.Add(new LdapNode(
                    FormatName(child),
                    GetDistinguishedName(child),
                    child.Path,
                    hasChildren: HasContainerChildren(child)));
            }
        }

        children.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return children;
    }

    private static bool HasContainerChildren(DirectoryEntry entry)
    {
        foreach (DirectoryEntry child in entry.Children)
        {
            using (child)
            {
                if (IsContainer(child))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsContainer(DirectoryEntry entry) =>
        Array.IndexOf(ContainerClasses, entry.SchemaClassName) >= 0;

    private static string GetDistinguishedName(DirectoryEntry entry) =>
        entry.Properties["distinguishedName"].Value as string ?? entry.Name;

    private static string FormatName(DirectoryEntry entry)
    {
        // entry.Name looks like "OU=Sales" or "DC=contoso" - keep only the value part.
        var name = entry.Name;
        var separatorIndex = name.IndexOf('=');
        return separatorIndex >= 0 && separatorIndex < name.Length - 1
            ? name[(separatorIndex + 1)..]
            : name;
    }
}
