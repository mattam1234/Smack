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
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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

        if (!Uri.TryCreate(server.ServerUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("Remote ServerUrl is not a valid absolute URI.");
        }

        baseUri = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
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
                var id = item.TryGetProperty("Id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
                var name = item.TryGetProperty("Name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;

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

        if (!Uri.TryCreate(server.ServerUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("Remote ServerUrl is not a valid absolute URI.");
        }

        baseUri = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
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
                var id = item.TryGetProperty("Id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var name = item.TryGetProperty("Name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
                var parent = item.TryGetProperty("ParentId", out var parentProp) ? parentProp.GetString() ?? string.Empty : string.Empty;
                var type = item.TryGetProperty("Type", out var typeProp) ? typeProp.GetString() ?? string.Empty : string.Empty;
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The absolute URL that can be used to stream the item, or null if unavailable.</returns>
    public Task<Uri?> GetStreamUrlAsync(SmackRemoteServer server, string itemId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return Task.FromResult<Uri?>(null);
        }

        if (!Uri.TryCreate(server.ServerUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("Remote ServerUrl is not a valid absolute URI.");
        }

        baseUri = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);

        var relative = "Items/" + Uri.EscapeDataString(itemId) + "/Download?api_key=" + Uri.EscapeDataString(server.ApiKey);
        var streamUri = new Uri(baseUri, relative);

        return Task.FromResult<Uri?>(streamUri);
    }
}
