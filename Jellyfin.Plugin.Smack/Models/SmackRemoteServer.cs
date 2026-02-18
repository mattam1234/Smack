using System;

namespace Jellyfin.Plugin.Smack.Models;

/// <summary>
/// Represents a remote Jellyfin server configuration used by the Smack plugin.
/// Contains the necessary information to connect to and authenticate with a remote Jellyfin instance.
/// </summary>
public class SmackRemoteServer
{
    /// <summary>
    /// Gets or sets the unique identifier for this remote server configuration.
    /// This ID is automatically generated when a new server is created and is used
    /// to reference the server throughout the plugin.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the friendly display name for the remote server.
    /// This name is shown in the UI to help identify the server.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL of the remote Jellyfin server.
    /// Must be a valid absolute URL starting with http:// or https://.
    /// Example: https://remote.example.com/jellyfin.
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key for authenticating with the remote Jellyfin server.
    /// This key should be generated from a user account on the remote server with
    /// appropriate permissions to access the desired media libraries.
    /// </summary>
    /// <remarks>
    /// Warning: The API key is stored in plain text in the Jellyfin configuration.
    /// Use a dedicated API key with minimal permissions for security.
    /// </remarks>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID associated with the API key on the remote server.
    /// This field may be populated automatically when connecting to the remote server,
    /// but is primarily informational and not currently used for API calls.
    /// </summary>
    public string RemoteUserId { get; set; } = string.Empty;
}
