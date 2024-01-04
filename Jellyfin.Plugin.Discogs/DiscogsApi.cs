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
    private readonly PluginConfiguration _configuration;
    private readonly HttpClient _client;

    public DiscogsApi(IHttpClientFactory clientFactory) : this(clientFactory, Plugin.Instance!.Configuration)
    {
    }

    public DiscogsApi(IHttpClientFactory clientFactory, PluginConfiguration configuration)
    {
        _client = clientFactory.CreateClient(NamedClient.Default);
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Plugin.Instance!.Name, Plugin.Instance!.Version.ToString()));
        _configuration = configuration;
    }

    private void AddRequestHeaders(HttpRequestMessage request)
    {
        // Remove default accept headers and add the Discogs specific one
        request.Headers.Accept.Clear();
        switch (_configuration.TextFormat)
        {
            default:
            case DiscogsTextFormat.PlainText:
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.discogs.v2.plaintext+json", 1.0));
                break;
            case DiscogsTextFormat.Html:
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.discogs.v2.html+json", 1.0));
                break;
        }

        // Authorize request
        request.Headers.Authorization = new AuthenticationHeaderValue("Discogs", $"token={_configuration.ApiToken}");
    }

    public async Task<JsonNode?> GetArtist(string id, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{_configuration.ApiServer}artists/{HttpUtility.UrlEncode(id)}");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        AddRequestHeaders(request);
        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonNode?> Search(string query, string? type, CancellationToken cancellationToken)
    {
        var uri = new Uri(QueryHelpers.AddQueryString($"{_configuration.ApiServer}database/search", new Dictionary<string, string?> { { "q", query }, { "type", type } }));
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        AddRequestHeaders(request);
        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> GetImage(string url, CancellationToken cancellationToken)
    {
        if (new Uri(url).Host != new Uri(_configuration.ImageServer).Host)
        {
            throw new ArgumentException($"Host does not match {_configuration.ImageServer}", nameof(url));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddRequestHeaders(request);
        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return response;
    }
}
