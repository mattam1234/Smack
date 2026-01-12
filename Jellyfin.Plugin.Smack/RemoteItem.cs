using System;

namespace Jellyfin.Plugin.Smack;

/// <summary>
/// Minimal remote item DTO used by the Smack plugin.
/// </summary>
public sealed class RemoteItem
{
    /// <summary>
    /// Gets or sets the remote item id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote item name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote item's parent id.
    /// </summary>
    public string ParentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote item type (e.g. 'Movie', 'Series', 'Folder').
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this item is a folder.
    /// </summary>
    public bool IsFolder { get; set; }
}
