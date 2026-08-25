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
folder next to `TechBox.exe` and loads every `.dll` found there that contains a public class
implementing `TechBox.Plugins.ITechBoxPlugin`.

A plugin can:
- register its own pages, view models and services into TechBox's dependency injection container
  (`ITechBoxPlugin.ConfigureServices`);
- contribute navigation menu items pointing to those pages (`ITechBoxPlugin.CreateMenuItems`);
- optionally override the application title and logo (`ApplicationTitle`, `LogoImagePath`).

To ship a plugin, build it as a class library targeting `net10.0-windows` with `UseWPF` enabled,
reference `TechBox.dll` (or the `TechBox` project) to implement `ITechBoxPlugin`, and drop the
compiled output (with its dependencies) in TechBox's `Plugins` folder.

See [Lear-Roche-La-Moliere/TechBox.Lear](https://github.com/Lear-Roche-La-Moliere/TechBox.Lear) for
an example plugin.