using System.Collections.Generic;
using TechBox.Models;

namespace TechBox.Services;

public interface ILdapTreeService
{
    /// <summary>Returns the root node of the tree (the domain's default naming context).</summary>
    LdapNode GetRootNode();

    /// <summary>Returns the immediate container children (OUs, containers, ...) of the given node.</summary>
    IReadOnlyList<LdapNode> GetChildren(LdapNode node);
}
