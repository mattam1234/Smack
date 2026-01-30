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
    public void GetStreamUrl_BuildsExpectedUrl()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "https://remote.example.com/jellyfin",
            ApiKey = "ABC123"
        };

        var itemId = "item-42";

        var uri = client.GetStreamUrl(server, itemId);

        Assert.NotNull(uri);
        Assert.Equal(
            "https://remote.example.com/jellyfin/Items/item-42/Download?api_key=ABC123",
            uri!.ToString());
    }

    [Fact]
    public void GetStreamUrl_ReturnsNull_WhenItemIdMissing()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "https://remote.example.com",
            ApiKey = "key"
        };

        var uri = client.GetStreamUrl(server, string.Empty);

        Assert.Null(uri);
    }

    [Fact]
    public void GetStreamUrl_Throws_WhenServerUrlInvalid()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "not-a-url",
            ApiKey = "key"
        };

        Assert.Throws<InvalidOperationException>(() =>
            client.GetStreamUrl(server, "item"));
    }

    [Fact]
    public void GetStreamUrl_NormalizesTrailingSlash()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "https://remote.example.com/jellyfin/",
            ApiKey = "key"
        };

        var uri = client.GetStreamUrl(server, "item-1");

        Assert.NotNull(uri);
        Assert.Equal("https://remote.example.com/jellyfin/Items/item-1/Download?api_key=key", uri!.ToString());
    }

    [Fact]
    public void GetStreamUrl_EscapesItemIdSpecialCharacters()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "https://remote.example.com",
            ApiKey = "simplekey"
        };

        var uri = client.GetStreamUrl(server, "item&special");

        Assert.NotNull(uri);
        // The item ID should be properly escaped in the path
        Assert.Contains("Items/item%26special/Download", uri!.ToString());
    }

    [Fact]
    public async Task GetLibrariesAsync_Throws_WhenServerNull()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.GetLibrariesAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GetLibrariesAsync_Throws_WhenServerUrlInvalid()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "not-a-url",
            ApiKey = "key"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetLibrariesAsync(server, CancellationToken.None));
    }

    [Fact]
    public async Task GetItemsAsync_Throws_WhenServerNull()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.GetItemsAsync(null!, "parent-id", CancellationToken.None));
    }

    [Fact]
    public async Task GetItemsAsync_Throws_WhenServerUrlInvalid()
    {
        using var httpClient = new HttpClient();
        var client = new SmackRemoteClient(httpClient);

        var server = new SmackRemoteServer
        {
            ServerUrl = "invalid",
            ApiKey = "key"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetItemsAsync(server, "parent-id", CancellationToken.None));
    }
}
