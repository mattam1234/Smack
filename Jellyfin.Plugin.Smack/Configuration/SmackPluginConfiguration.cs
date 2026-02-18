using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jellyfin.Plugin.Smack.Models;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Smack.Configuration;

/// <summary>
/// Smack plugin configuration root containing remote server configurations.
/// This configuration is persisted by Jellyfin and managed through the plugin's configuration page.
/// </summary>
public class SmackPluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets the list of configured remote Jellyfin servers.
    /// Each server represents a remote Jellyfin instance that can be browsed and streamed from.
    /// Modifications to this collection are automatically saved by Jellyfin when the configuration is updated.
    /// </summary>
    /// <remarks>
    /// This collection should be modified through the Smack configuration UI to ensure proper validation
    /// and to maintain consistency across the plugin's functionality.
    /// </remarks>
    public Collection<SmackRemoteServer> RemoteServers { get; } = new();
}
