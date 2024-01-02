using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Jellyfin.Plugin.Discogs.Configuration;
using MediaBrowser.Common.Net;
using Microsoft.AspNetCore.WebUtilities;

namespace Jellyfin.Plugin.Discogs;

#pragma warning disable CS1591
public class DiscogsApi
{
    private const string Server = "https://api.discogs.com/";
    private readonly HttpClient _client;

    public DiscogsApi(IHttpClientFactory clientFactory) : this(clientFactory, Plugin.Instance!.Configuration)
    {
    }

    public DiscogsApi(IHttpClientFactory clientFactory, PluginConfiguration configuration)
    {
        _client = clientFactory.CreateClient(NamedClient.Default);

        // TODO: This doesn't update the token when configuration changes
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Discogs", $"token={configuration.ApiToken}");
    }

    public async Task<JsonNode?> GetArtist(string id, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{Server}artists/{HttpUtility.UrlEncode(id)}");
        var response = await _client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonNode?> Search(string query, string? type, CancellationToken cancellationToken)
    {
        var uri = new Uri(QueryHelpers.AddQueryString($"{Server}database/search", new Dictionary<string, string?> { { "q", query }, { "type", type } }));
        var response = await _client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> GetImage(string url, CancellationToken cancellationToken)
    {
        if (!url.StartsWith(Server, StringComparison.Ordinal))
        {
            throw new ArgumentException($"URL does not start with {Server}", nameof(url));
        }

        var response = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return response;
    }
}
