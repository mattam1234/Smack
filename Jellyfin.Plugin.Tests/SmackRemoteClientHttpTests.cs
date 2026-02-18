using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Smack;
using Jellyfin.Plugin.Smack.Models;
using Moq;
using Moq.Protected;

namespace Jellyfin.Plugin.Tests;

/// <summary>
/// Tests for SmackRemoteClient with mocked HTTP responses.
/// </summary>
public class SmackRemoteClientHttpTests
{
    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string responseContent)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task GetLibrariesAsync_ParsesValidResponse()
    {
        // Arrange
        var jsonResponse = """
            {
                "Items": [
                    { "Id": "lib1", "Name": "Movies" },
                    { "Id": "lib2", "Name": "TV Shows" },
                    { "Id": "lib3", "Name": "Music" }
                ]
            }
            """;

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var libraries = await client.GetLibrariesAsync(server, CancellationToken.None);

        // Assert
        Assert.NotNull(libraries);
        Assert.Equal(3, libraries.Count);
        Assert.Equal("lib1", libraries[0].Id);
        Assert.Equal("Movies", libraries[0].Name);
        Assert.Equal("lib2", libraries[1].Id);
        Assert.Equal("TV Shows", libraries[1].Name);
        Assert.Equal("lib3", libraries[2].Id);
        Assert.Equal("Music", libraries[2].Name);
    }

    [Fact]
    public async Task GetLibrariesAsync_HandlesEmptyResponse()
    {
        // Arrange
        var jsonResponse = """{ "Items": [] }""";

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var libraries = await client.GetLibrariesAsync(server, CancellationToken.None);

        // Assert
        Assert.NotNull(libraries);
        Assert.Empty(libraries);
    }

    [Fact]
    public async Task GetLibrariesAsync_SkipsItemsWithoutId()
    {
        // Arrange
        var jsonResponse = """
            {
                "Items": [
                    { "Id": "lib1", "Name": "Movies" },
                    { "Name": "NoId" },
                    { "Id": "", "Name": "EmptyId" },
                    { "Id": "lib2", "Name": "TV Shows" }
                ]
            }
            """;

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var libraries = await client.GetLibrariesAsync(server, CancellationToken.None);

        // Assert
        Assert.NotNull(libraries);
        Assert.Equal(2, libraries.Count);
        Assert.Equal("lib1", libraries[0].Id);
        Assert.Equal("lib2", libraries[1].Id);
    }

    [Fact]
    public async Task GetLibrariesAsync_HandlesHttpError()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(HttpStatusCode.Unauthorized, "Unauthorized");
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "invalid-key"
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.GetLibrariesAsync(server, CancellationToken.None));
    }

    [Fact]
    public async Task GetItemsAsync_ParsesValidResponse()
    {
        // Arrange
        var jsonResponse = """
            {
                "Items": [
                    {
                        "Id": "item1",
                        "Name": "Folder 1",
                        "ParentId": "parent1",
                        "Type": "Folder",
                        "IsFolder": true
                    },
                    {
                        "Id": "item2",
                        "Name": "Movie 1",
                        "ParentId": "parent1",
                        "Type": "Movie",
                        "IsFolder": false
                    },
                    {
                        "Id": "item3",
                        "Name": "Episode 1",
                        "ParentId": "parent1",
                        "Type": "Episode"
                    }
                ]
            }
            """;

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var items = await client.GetItemsAsync(server, "parent1", CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Equal(3, items.Count);

        // First item (folder)
        Assert.Equal("item1", items[0].Id);
        Assert.Equal("Folder 1", items[0].Name);
        Assert.Equal("parent1", items[0].ParentId);
        Assert.Equal("Folder", items[0].Type);
        Assert.True(items[0].IsFolder);

        // Second item (movie)
        Assert.Equal("item2", items[1].Id);
        Assert.Equal("Movie 1", items[1].Name);
        Assert.False(items[1].IsFolder);

        // Third item (no IsFolder property)
        Assert.Equal("item3", items[2].Id);
        Assert.False(items[2].IsFolder);
    }

    [Fact]
    public async Task GetItemsAsync_SkipsItemsWithoutId()
    {
        // Arrange
        var jsonResponse = """
            {
                "Items": [
                    { "Id": "item1", "Name": "Valid Item" },
                    { "Name": "No Id Item" },
                    { "Id": "", "Name": "Empty Id Item" },
                    { "Id": "item2", "Name": "Another Valid Item" }
                ]
            }
            """;

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var items = await client.GetItemsAsync(server, "parent1", CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
        Assert.Equal("item1", items[0].Id);
        Assert.Equal("item2", items[1].Id);
    }

    [Fact]
    public async Task GetItemsAsync_HandlesEmptyParentId()
    {
        // Arrange
        var jsonResponse = """{ "Items": [] }""";

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var items = await client.GetItemsAsync(server, string.Empty, CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetItemsAsync_HandlesNullParentId()
    {
        // Arrange
        var jsonResponse = """{ "Items": [] }""";

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var items = await client.GetItemsAsync(server, null!, CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetItemsAsync_HandlesHttpError()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(HttpStatusCode.NotFound, "Not Found");
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.GetItemsAsync(server, "parent1", CancellationToken.None));
    }

    [Fact]
    public async Task GetLibrariesAsync_HandlesNoItemsProperty()
    {
        // Arrange
        var jsonResponse = """{ "SomeOtherProperty": "value" }""";

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var libraries = await client.GetLibrariesAsync(server, CancellationToken.None);

        // Assert
        Assert.NotNull(libraries);
        Assert.Empty(libraries);
    }

    [Fact]
    public async Task GetItemsAsync_HandlesNoItemsProperty()
    {
        // Arrange
        var jsonResponse = """{ "TotalRecordCount": 0 }""";

        using var httpClient = CreateMockHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new SmackRemoteClient(httpClient);
        var server = new SmackRemoteServer
        {
            ServerUrl = "https://example.com",
            ApiKey = "test-key"
        };

        // Act
        var items = await client.GetItemsAsync(server, "parent1", CancellationToken.None);

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }
}
