# TechBox
Open Source IT Technician box.

# Requirements

- Windows 11
- DotNet 10.0 Desktop
- A computer on a domain

# Features

[] Read user informations from Domain
[] Backup/Restore user profile
[] Read computer informations
[] Execute CCM actions on remote computer
[] Remote computer throught RDP or CCM Remote Viewer
[] Remote power off or reboot computer

# TODO

[] Integrate GLPI

# Plugins

TechBox can be extended without modifying its source code. At startup it scans a `Plugins`
folder next to `TechBox.exe` (created automatically if missing) one level deep: each subfolder,
`Plugins\{PluginName}\`, is one isolated plugin, and every `.dll` found directly inside it that
contains a public class implementing `TechBox.PluginContract.ITechBoxPlugin` is loaded. Keeping
each plugin in its own subfolder means its DLLs never mix with another plugin's, and adding or
removing a plugin is just adding or deleting its folder. A plugin that fails to load is skipped
and reported in a warning dialog; it never prevents the rest of the application, or other
plugins, from starting.

A plugin can:
- register its own pages, view models and services into TechBox's dependency injection container
  (`ITechBoxPlugin.ConfigureServices`);
- contribute navigation menu items pointing to those pages (`ITechBoxPlugin.CreateMenuItems`);
- optionally override the application title and logo (`ApplicationTitle`, `LogoImagePath`).

## Writing a plugin

1. Create a class library targeting `net10.0-windows` with `<UseWPF>true</UseWPF>` and
   `<EnableDynamicLoading>true</EnableDynamicLoading>` (the latter makes `dotnet build`, not just
   `dotnet publish`, copy the plugin's own dependencies next to its DLL). Add a project (or
   package) reference to `TechBox.PluginContract` (`TechBox.PluginContract\TechBox.PluginContract.csproj`)
   — plugin authors only need this lightweight contract assembly, not the full `TechBox` project.
   If you use a `ProjectReference`, mark it `Private="false"` with `ExcludeAssets="runtime"` so
   your plugin's build output doesn't ship its own copy of `TechBox.PluginContract.dll` — TechBox
   resolves that assembly from its own copy at load time, and a duplicate copy next to your plugin
   would break `is ITechBoxPlugin` type checks.
2. Implement `TechBox.PluginContract.ITechBoxPlugin` on a public class with a public parameterless
   constructor, registering its pages/view models/services in `ConfigureServices` and contributing
   navigation menu items in `CreateMenuItems`.
3. Build the project and copy its entire build output (the plugin DLL, its `.deps.json`, and any
   dependency DLLs) into its own subfolder of TechBox's `Plugins` folder, e.g.
   `Plugins\MyPlugin\`. DLLs that aren't plugins themselves (dependencies) are simply ignored by
   the scan; keeping each plugin in its own subfolder keeps its dependencies from clashing with
   another plugin's.

A working, buildable example lives in `samples/TechBox.SamplePlugin`.

See also [Lear-Roche-La-Moliere/TechBox.Lear](https://github.com/Lear-Roche-La-Moliere/TechBox.Lear)
for a real-world plugin.