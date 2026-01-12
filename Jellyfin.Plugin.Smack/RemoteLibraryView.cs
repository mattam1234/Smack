using System;

namespace Jellyfin.Plugin.Smack;

/// <summary>
/// Minimal remote library view DTO returned from remote Jellyfin servers.
/// </summary>
public sealed class RemoteLibraryView
{
    /// <summary>
    /// Gets or sets the remote library id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote library name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
