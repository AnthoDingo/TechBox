// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using TechBox.PluginContract;

namespace TechBox.Plugins
{
    /// <summary>
    /// Discovers and loads TechBox plugins: each subfolder of the application's <c>Plugins</c>
    /// folder is treated as one isolated plugin, and every DLL directly inside that subfolder is
    /// scanned for public, non-abstract types implementing <see cref="ITechBoxPlugin"/>, which are
    /// instantiated. DLLs with no such type (e.g. a plugin's own dependency assemblies) are ignored.
    /// </summary>
    public sealed class PluginManager
    {
        private readonly List<ITechBoxPlugin> _plugins = new();
        private readonly List<PluginLoadError> _errors = new();

        public IReadOnlyList<ITechBoxPlugin> Plugins => _plugins;

        public IReadOnlyList<PluginLoadError> Errors => _errors;

        /// <summary>
        /// Loads every plugin found in a subfolder of <paramref name="pluginsDirectory"/>
        /// (<c>Plugins\{PluginName}\*.dll</c>), creating the <c>Plugins</c> folder if it doesn't
        /// exist yet. Each subfolder is scanned independently, keeping one plugin's DLLs from
        /// mixing with another's, so adding or removing a plugin is just adding or deleting its
        /// folder. A plugin that fails to load is skipped and recorded in <see cref="Errors"/>; it
        /// never prevents the other plugins from loading.
        /// </summary>
        public void LoadPlugins(string pluginsDirectory)
        {
            Directory.CreateDirectory(pluginsDirectory);

            foreach (string pluginDirectory in Directory.EnumerateDirectories(pluginsDirectory))
            {
                foreach (string dllPath in Directory.EnumerateFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly))
                {
                    LoadPlugin(dllPath);
                }
            }
        }

        private void LoadPlugin(string dllPath)
        {
            string fileName = Path.GetFileName(dllPath);

            // A plugin folder built with EnableDynamicLoading contains not just the plugin's own
            // DLL but every one of its dependencies, including shared ones like WPF-UI or
            // TechBox.PluginContract that the host has already loaded. Loading such a DLL here
            // would call LoadFromAssemblyPath directly, which bypasses PluginAssemblyLoadContext's
            // Load() override entirely and creates a second, distinct copy of that assembly before
            // the host even gets a chance to defer to its own - producing CLR types (like
            // NavigationViewItem) that WPF's styling then refuses to treat as compatible. Skip any
            // DLL whose assembly name is already loaded in the default context; it can only be one
            // of those shared dependencies, never a plugin's own assembly.
            string assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            if (AssemblyLoadContext.Default.Assemblies.Any(assembly => assembly.GetName().Name == assemblyName))
            {
                return;
            }

            try
            {
                PluginAssemblyLoadContext loadContext = new(dllPath);
                Assembly assembly = loadContext.LoadFromAssemblyPath(dllPath);

                // A DLL with no ITechBoxPlugin implementation is silently skipped rather than
                // reported: it is normal for a plugin's own dependency DLLs to sit next to it in
                // this folder.
                foreach (Type pluginType in GetLoadablePluginTypes(assembly))
                {
                    LoadPluginType(fileName, pluginType);
                }
            }
            catch (Exception ex)
            {
                // Loading a third-party plugin DLL can fail in many ways (missing dependency,
                // corrupt file, incompatible runtime, ...); one bad plugin must not take the whole
                // application down, so every failure is recorded instead of propagated.
                _errors.Add(new PluginLoadError(fileName, ex.Message));
            }
        }

        private static List<Type> GetLoadablePluginTypes(Assembly assembly)
        {
            Type?[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            return types
                .Where(type => type is { IsPublic: true, IsClass: true, IsAbstract: false })
                .Where(type => typeof(ITechBoxPlugin).IsAssignableFrom(type))
                .Where(type => type!.GetConstructor(Type.EmptyTypes) is not null)
                .Select(type => type!)
                .ToList();
        }

        private void LoadPluginType(string fileName, Type pluginType)
        {
            string pluginName = fileName;

            try
            {
                if (Activator.CreateInstance(pluginType) is ITechBoxPlugin plugin)
                {
                    pluginName = plugin.Name;
                    _plugins.Add(plugin);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(new PluginLoadError(fileName, $"{pluginName} : {ex.Message}"));
            }
        }
    }
}
