// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace TechBox.Plugins
{
    /// <summary>
    /// Isolated load context for one plugin DLL. Resolves the plugin's own dependencies (other
    /// assemblies/native libraries next to it) without shadowing assemblies already loaded by the
    /// host, such as <c>TechBox.PluginContract</c> or the WPF assemblies, which resolve to the
    /// host's copies so type checks like <c>is ITechBoxPlugin</c> work across the load context boundary.
    /// </summary>
    internal sealed class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginAssemblyLoadContext(string pluginPath)
            : base(name: Path.GetFileNameWithoutExtension(pluginPath), isCollectible: false)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // If the host already has an assembly of this name loaded (WPF assemblies, WPF-UI,
            // TechBox.PluginContract, ...), defer to it instead of loading a second copy from this
            // plugin's own output folder. Without this, a NuGet dependency the plugin ships next to
            // itself (e.g. WPF-UI, when EnableDynamicLoading copies it there) would be loaded twice
            // as two distinct CLR types, and WPF would throw "Can only base on a Style with target
            // type that is base type '...'" when styling a control created from the plugin's copy.
            if (AssemblyLoadContext.Default.Assemblies.Any(assembly => assembly.GetName().Name == assemblyName.Name))
            {
                return null;
            }

            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
        }

        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return libraryPath is null ? nint.Zero : LoadUnmanagedDllFromPath(libraryPath);
        }
    }
}
