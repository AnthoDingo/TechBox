# Plugins

Drop each plugin's build output (its `.dll` and dependencies) in its own subfolder of `Plugins`,
next to `TechBox.exe`: `Plugins\{PluginName}\`. TechBox scans one level deep on startup and loads
every assembly it finds inside each plugin's subfolder, keeping plugins isolated from one another
so their DLLs never mix. Adding a plugin is copying in a folder; removing one is deleting it.

See the main [README](../../README.md#plugins) for how to write a plugin.
