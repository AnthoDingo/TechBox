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
