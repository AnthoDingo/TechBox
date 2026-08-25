// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Reflection;
using TechBox.PluginContract;

namespace TechBox.Plugins
{
    /// <summary>
    /// Discovers and loads TechBox plugins: every DLL directly inside the application's
    /// <c>Plugins</c> folder is scanned for public, non-abstract types implementing
    /// <see cref="ITechBoxPlugin"/>, which are instantiated. DLLs with no such type (e.g. a
    /// plugin's own dependency assemblies) are ignored.
    /// </summary>
    public sealed class PluginManager
    {
        private readonly List<ITechBoxPlugin> _plugins = new();
        private readonly List<PluginLoadError> _errors = new();

        public IReadOnlyList<ITechBoxPlugin> Plugins => _plugins;

        public IReadOnlyList<PluginLoadError> Errors => _errors;

        /// <summary>
        /// Loads every plugin DLL found directly inside <paramref name="pluginsDirectory"/>,
        /// creating the folder if it doesn't exist yet. A plugin that fails to load is skipped and
        /// recorded in <see cref="Errors"/>; it never prevents the other plugins from loading.
        /// </summary>
        public void LoadPlugins(string pluginsDirectory)
        {
            Directory.CreateDirectory(pluginsDirectory);

            foreach (string dllPath in Directory.EnumerateFiles(pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly))
            {
                LoadPlugin(dllPath);
            }
        }

        private void LoadPlugin(string dllPath)
        {
            string fileName = Path.GetFileName(dllPath);

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
