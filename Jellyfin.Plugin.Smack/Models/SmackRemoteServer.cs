using System;

namespace Jellyfin.Plugin.Smack.Models;

/// <summary>
/// Represents a remote Jellyfin server configuration used by the Smack plugin.
/// </summary>
public class SmackRemoteServer
{
    /// <summary>
    /// Gets or sets the unique identifier for the remote server configuration.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the name for the remote server configuration.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server url for the remote server configuration.
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the api key for the remote server configuration.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote user id for the remote server configuration.
    /// </summary>
    public string RemoteUserId { get; set; } = string.Empty;
}
