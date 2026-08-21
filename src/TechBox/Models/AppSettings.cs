namespace TechBox.Models;

/// <summary>
/// Application settings persisted between launches.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// ADsPath (e.g. "LDAP://OU=Sales,DC=contoso,DC=com") of the LDAP location selected by the user.
    /// Null/empty means no location has been selected yet, which triggers the mandatory selection window.
    /// </summary>
    public string? LdapPath { get; set; }

    /// <summary>
    /// Distinguished name of the selected LDAP location, kept for display purposes.
    /// </summary>
    public string? LdapDistinguishedName { get; set; }
}
