using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Smack.Models;

namespace Jellyfin.Plugin.Smack;

/// <summary>
/// Simple HTTP client wrapper for talking to remote Jellyfin servers configured for Smack.
/// </summary>
public class SmackRemoteClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmackRemoteClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance.</param>
    public SmackRemoteClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets available library views on a remote Jellyfin server.
    /// </summary>
    /// <param name="server">The remote server configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of remote library views.</returns>
    public async Task<IReadOnlyList<RemoteLibraryView>> GetLibrariesAsync(SmackRemoteServer server, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);

        var baseUri = GetValidatedBaseUri(server);
        var requestUri = new Uri(baseUri, "Users/Me/Views?api_key=" + Uri.EscapeDataString(server.ApiKey));

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        var root = json.RootElement;

        var list = new List<RemoteLibraryView>();

        if (root.TryGetProperty("Items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in itemsElement.EnumerateArray())
            {
                var id = GetStringProperty(item, "Id");
                var name = GetStringProperty(item, "Name");

                if (!string.IsNullOrEmpty(id))
                {
                    list.Add(new RemoteLibraryView
                    {
                        Id = id,
                        Name = name
                    });
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Gets items under a given parent on a remote Jellyfin server.
    /// </summary>
    /// <param name="server">The remote server configuration.</param>
    /// <param name="parentId">The parent item id (library id, folder id, etc.).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of remote items.</returns>
    public async Task<IReadOnlyList<RemoteItem>> GetItemsAsync(SmackRemoteServer server, string parentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);

        var baseUri = GetValidatedBaseUri(server);
        parentId ??= string.Empty;

        var query = "Users/Me/Items?ParentId=" + Uri.EscapeDataString(parentId) +
                    "&Fields=BasicSyncInfo&api_key=" + Uri.EscapeDataString(server.ApiKey);

        var requestUri = new Uri(baseUri, query);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        var root = json.RootElement;

        var list = new List<RemoteItem>();

        if (root.TryGetProperty("Items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in itemsElement.EnumerateArray())
            {
                var id = GetStringProperty(item, "Id");
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var name = GetStringProperty(item, "Name");
                var parent = GetStringProperty(item, "ParentId");
                var type = GetStringProperty(item, "Type");
                var isFolder = item.TryGetProperty("IsFolder", out var folderProp) && folderProp.ValueKind == JsonValueKind.True;

                list.Add(new RemoteItem
                {
                    Id = id,
                    Name = name,
                    ParentId = parent,
                    Type = type,
                    IsFolder = isFolder
                });
            }
        }

        return list;
    }

    /// <summary>
    /// Gets a basic stream URL for a remote item.
    /// </summary>
    /// <param name="server">The remote server configuration.</param>
    /// <param name="itemId">The remote item id.</param>
    /// <returns>The absolute URL that can be used to stream the item, or null if itemId is empty.</returns>
    public Uri? GetStreamUrl(SmackRemoteServer server, string itemId)
    {
        ArgumentNullException.ThrowIfNull(server);

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        var baseUri = GetValidatedBaseUri(server);

        var relative = "Items/" + Uri.EscapeDataString(itemId) + "/Download?api_key=" + Uri.EscapeDataString(server.ApiKey);
        return new Uri(baseUri, relative);
    }

    /// <summary>
    /// Validates and normalizes the server URL to an absolute URI with trailing slash.
    /// </summary>
    /// <param name="server">The remote server configuration.</param>
    /// <returns>The validated base URI.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the server URL is not a valid absolute URI.</exception>
    private static Uri GetValidatedBaseUri(SmackRemoteServer server)
    {
        if (!Uri.TryCreate(server.ServerUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("Remote ServerUrl is not a valid absolute URI.");
        }

        return new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
    }

    /// <summary>
    /// Gets a string property from a JSON element, returning an empty string if not found.
    /// </summary>
    /// <param name="element">The JSON element.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property value or an empty string.</returns>
    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() ?? string.Empty : string.Empty;
    }
}
