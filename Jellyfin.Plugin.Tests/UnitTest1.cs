using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Smack;
using Jellyfin.Plugin.Smack.Models;

namespace Jellyfin.Plugin.Tests;

public class SmackRemoteClientTests
{
    [Fact]
    public async Task GetStreamUrlAsync_BuildsExpectedUrl()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "https://remote.example.com/jellyfin",
            ApiKey = "ABC123"
        };

        var itemId = "item-42";

        var uri = await client.GetStreamUrlAsync(server, itemId, CancellationToken.None);

        Assert.NotNull(uri);
        Assert.Equal(
            "https://remote.example.com/jellyfin/Items/item-42/Download?api_key=ABC123",
            uri!.ToString());
    }

    [Fact]
    public async Task GetStreamUrlAsync_ReturnsNull_WhenItemIdMissing()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "https://remote.example.com",
            ApiKey = "key"
        };

        var uri = await client.GetStreamUrlAsync(server, string.Empty, CancellationToken.None);

        Assert.Null(uri);
    }

    [Fact]
    public async Task GetStreamUrlAsync_Throws_WhenServerUrlInvalid()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "not-a-url",
            ApiKey = "key"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetStreamUrlAsync(server, "item", CancellationToken.None));
    }
}
