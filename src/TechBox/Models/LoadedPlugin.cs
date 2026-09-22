namespace TechBox.Models
{
    /// <summary>
    /// A plugin loaded at startup, as the settings page shows it.
    /// </summary>
    /// <param name="Name">Name the plugin reports through <c>ITechBoxPlugin.Name</c>.</param>
    /// <param name="Version">Version of the assembly it lives in.</param>
    /// <param name="FileName">File the plugin was loaded from, without its folder.</param>
    public sealed record LoadedPlugin(string Name, string Version, string FileName);
}
