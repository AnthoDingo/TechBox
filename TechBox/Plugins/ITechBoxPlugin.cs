// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace TechBox.Plugins
{
    /// <summary>
    /// Contract implemented by TechBox plugins. A plugin is a class library (.dll) dropped in the
    /// application's "Plugins" folder, containing exactly one public, parameterless-constructible
    /// implementation of this interface. It is discovered and loaded at startup by <see cref="PluginLoader"/>.
    /// </summary>
    public interface ITechBoxPlugin
    {
        /// <summary>
        /// Display name of the plugin, used for diagnostics and logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Optional application title override. When multiple plugins provide one, the first loaded wins.
        /// </summary>
        string? ApplicationTitle => null;

        /// <summary>
        /// Optional absolute path to a logo image used to replace the default TechBox branding
        /// (title bar icon and splash screen). When multiple plugins provide one, the first loaded wins.
        /// </summary>
        string? LogoImagePath => null;

        /// <summary>
        /// Registers the plugin's pages, view models and services into the application's dependency
        /// injection container. Any <see cref="Wpf.Ui.Controls.INavigableView{T}"/> page referenced by
        /// <see cref="CreateMenuItems"/> must be registered here for navigation to be able to resolve it.
        /// </summary>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Creates the navigation menu items contributed by this plugin. They are appended to the
        /// main window's navigation pane after the built-in menu items.
        /// </summary>
        IEnumerable<NavigationViewItem> CreateMenuItems();
    }
}
