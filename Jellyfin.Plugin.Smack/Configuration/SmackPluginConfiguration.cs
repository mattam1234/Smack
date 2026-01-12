using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jellyfin.Plugin.Smack.Models;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Smack.Configuration;

/// <summary>
/// Smack plugin configuration root.
/// </summary>
public class SmackPluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets the list of configured remote Jellyfin servers.
    /// </summary>
    public Collection<SmackRemoteServer> RemoteServers { get; } = new();
}
