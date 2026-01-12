using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Smack;
using MediaBrowser.Controller.Net;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Smack.Controllers;

/// <summary>
/// API surface for the Smack plugin.
/// </summary>
[ApiController]
[Route("Smack")] // Routes will be /Smack/... under the Jellyfin API root
public class SmackController : ControllerBase
{
    private readonly SmackRemoteClient _remoteClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmackController"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for remote calls.</param>
    public SmackController(HttpClient httpClient)
    {
        _remoteClient = new SmackRemoteClient(httpClient);
    }

    /// <summary>
    /// Get configured remote servers (sanitized, without API keys).
    /// </summary>
    /// <returns>The list of configured remote servers.</returns>
    [HttpGet("Servers")]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    public ActionResult<IEnumerable<object>> GetServers()
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return Ok(Array.Empty<object>());
        }

        var servers = config.RemoteServers.Select(s => new
        {
            s.Id,
            s.Name,
            s.ServerUrl,
            s.RemoteUserId
        });

        return Ok(servers);
    }

    /// <summary>
    /// Get library views from a given remote server.
    /// </summary>
    /// <param name="serverId">The local identifier of the remote server.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of library views from the remote server.</returns>
    [HttpGet("Libraries/{serverId}")]
    [ProducesResponseType(typeof(IEnumerable<RemoteLibraryView>), 200)]
    public async Task<ActionResult<IEnumerable<RemoteLibraryView>>> GetLibraries(
    string serverId,
    CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return NotFound("Plugin configuration not available.");
        }

        var server = config.RemoteServers
            .FirstOrDefault(s => string.Equals(s.Id, serverId, StringComparison.OrdinalIgnoreCase));

        if (server == null)
        {
            return NotFound("Remote server not found.");
        }

        if (string.IsNullOrWhiteSpace(server.ServerUrl) ||
            string.IsNullOrWhiteSpace(server.ApiKey))
        {
            return BadRequest("Remote server is not fully configured.");
        }

        try
        {
            var libraries = await _remoteClient
                .GetLibrariesAsync(server, cancellationToken)
                .ConfigureAwait(false);

            return Ok(libraries);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, "Remote server error: " + ex.Message);
        }
    }

    /// <summary>
    /// Get items from a given remote server and parent id.
    /// </summary>
    /// <param name="serverId">The local identifier of the remote server.</param>
    /// <param name="parentId">The parent id on the remote server (library id or folder id).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of items from the remote server.</returns>
    [HttpGet("Items/{serverId}/{parentId}")]
    [ProducesResponseType(typeof(IEnumerable<RemoteItem>), 200)]
    public async Task<ActionResult<IEnumerable<RemoteItem>>> GetItems(string serverId, string parentId, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return NotFound("Plugin configuration not available.");
        }

        var server = config.RemoteServers.FirstOrDefault(s => string.Equals(s.Id, serverId, StringComparison.OrdinalIgnoreCase));
        if (server == null)
        {
            return NotFound("Remote server not found.");
        }

        if (string.IsNullOrWhiteSpace(server.ServerUrl) || string.IsNullOrWhiteSpace(server.ApiKey))
        {
            return BadRequest("Remote server is not fully configured.");
        }

        try
        {
            var items = await _remoteClient.GetItemsAsync(server, parentId, cancellationToken).ConfigureAwait(false);
            return Ok(items);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, "Remote server error: " + ex.Message);
        }
    }

    /// <summary>
    /// Get a basic stream URL for a remote item.
    /// </summary>
    /// <param name="serverId">The local identifier of the remote server.</param>
    /// <param name="itemId">The remote item id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An object containing the stream URL and basic metadata.</returns>
    [HttpGet("Stream/{serverId}/{itemId}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult<object>> GetStream(string serverId, string itemId, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return NotFound("Plugin configuration not available.");
        }

        var server = config.RemoteServers.FirstOrDefault(s => string.Equals(s.Id, serverId, StringComparison.OrdinalIgnoreCase));
        if (server == null)
        {
            return NotFound("Remote server not found.");
        }

        if (string.IsNullOrWhiteSpace(server.ServerUrl) || string.IsNullOrWhiteSpace(server.ApiKey))
        {
            return BadRequest("Remote server is not fully configured.");
        }

        try
        {
            var uri = await _remoteClient.GetStreamUrlAsync(server, itemId, cancellationToken).ConfigureAwait(false);
            if (uri == null)
            {
                return NotFound("Unable to build stream URL for remote item.");
            }

            return Ok(new
            {
                StreamUrl = uri.ToString(),
                ServerName = server.Name,
                ItemId = itemId,
                Protocol = "File",
                MediaType = "Video",
                Name = "Remote: " + (server.Name ?? "Server")
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, "Remote server error: " + ex.Message);
        }
    }
}
