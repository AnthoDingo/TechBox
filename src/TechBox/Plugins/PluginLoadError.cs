// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace TechBox.Plugins
{
    /// <summary>Describes why a plugin (or one of its types) failed to load.</summary>
    public sealed record PluginLoadError(string FileName, string Message);
}
