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

    /// <summary>
    /// Gets or sets the production year of the item, if available.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets a short overview/synopsis of the item, if available.
    /// </summary>
    public string Overview { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute URL to the primary thumbnail image for this item,
    /// including the remote server's API key as a query parameter. Empty when the
    /// item has no primary image.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the run time of the item in 100-nanosecond ticks (Jellyfin convention).
    /// Null when not available (e.g. folders).
    /// </summary>
    public long? RunTimeTicks { get; set; }

    /// <summary>
    /// Gets or sets the community rating of the item (0–10 scale from Jellyfin).
    /// Null when not available.
    /// </summary>
    public double? CommunityRating { get; set; }
}
