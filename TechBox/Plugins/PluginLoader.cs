// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace TechBox.Plugins
{
    /// <summary>
    /// Discovers and loads <see cref="ITechBoxPlugin"/> implementations from .dll files
    /// dropped in the "Plugins" folder next to the application executable.
    /// </summary>
    public static class PluginLoader
    {
        public static IReadOnlyList<ITechBoxPlugin> LoadPlugins()
        {
            List<ITechBoxPlugin> plugins = new();

            string pluginsDirectory = Path.Combine(AppContext.BaseDirectory, "Plugins");
            if (!Directory.Exists(pluginsDirectory))
                return plugins;

            foreach (string dllPath in Directory.GetFiles(pluginsDirectory, "*.dll"))
            {
                try
                {
                    PluginLoadContext context = new(dllPath);
                    Assembly assembly = context.LoadFromAssemblyPath(dllPath);

                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!typeof(ITechBoxPlugin).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
                            continue;

                        if (Activator.CreateInstance(type) is ITechBoxPlugin plugin)
                            plugins.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"TechBox: failed to load plugin '{dllPath}': {ex}");
                }
            }

            return plugins;
        }

        /// <summary>
        /// Isolated load context that resolves a plugin's own dependencies (from its .deps.json)
        /// without clashing with the host application's assemblies.
        /// </summary>
        private sealed class PluginLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            public PluginLoadContext(string pluginPath)
                : base(isCollectible: false)
            {
                _resolver = new AssemblyDependencyResolver(pluginPath);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
            }
        }
    }
}
