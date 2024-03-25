using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Web;
using Jellyfin.Plugin.Discogs.Configuration;
using MediaBrowser.Common.Net;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Discogs;

#pragma warning disable CS1591
public sealed class DiscogsApi : IDisposable
{
    private readonly ProductInfoHeaderValue _pluginUserAgent;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<DiscogsApi> _logger;
    private readonly PluginConfiguration _configuration;

    // Discogs allows 60 requests per minute when authenticated, we'll use 1 request per second instead.
    private readonly FixedWindowRateLimiter _rateLimiter = new(new FixedWindowRateLimiterOptions { Window = TimeSpan.FromSeconds(1), PermitLimit = 1, QueueLimit = 1000 });

    public DiscogsApi(IHttpClientFactory clientFactory, ILogger<DiscogsApi> logger) : this(clientFactory, logger, Plugin.Instance!.Configuration)
    {
    }

    public DiscogsApi(IHttpClientFactory clientFactory, ILogger<DiscogsApi> logger, PluginConfiguration configuration)
    {
        _pluginUserAgent = new ProductInfoHeaderValue(Plugin.Instance!.Name, Plugin.Instance.Version.ToString());
        _clientFactory = clientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    private async Task<HttpResponseMessage> Request(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var lease = await _rateLimiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);
        if (!lease.IsAcquired)
        {
            _logger.LogWarning("Rate limiter is overloaded!!");
            return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        }

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
        request.Headers.UserAgent.Add(_pluginUserAgent);

        // Do actual request
        var response = await _clientFactory.CreateClient(NamedClient.Default).SendAsync(request, cancellationToken).ConfigureAwait(false);

        // Realistically this shouldn't happen
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // Note: The image server does NOT return these headers
            response.Headers.TryGetValues("X-Discogs-Ratelimit", out var rateLimit);
            response.Headers.TryGetValues("X-Discogs-Ratelimit-Used", out var rateLimitUsed);
            response.Headers.TryGetValues("X-Discogs-Ratelimit-Remaining", out var rateLimitRemaining);
            _logger.LogWarning("It looks like we are rate limited. RateLimit={0}, RateLimitUsed={1}, RateLimitRemaining={2}", rateLimit?.FirstOrDefault(), rateLimitUsed?.FirstOrDefault(), rateLimitRemaining?.FirstOrDefault());
        }

        // Check for correct status before returning response
        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<JsonNode?> GetArtist(string id, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{_configuration.ApiServer}artists/{HttpUtility.UrlEncode(id)}");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await Request(request, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonNode?> GetRelease(string id, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{_configuration.ApiServer}releases/{HttpUtility.UrlEncode(id)}");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await Request(request, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonNode?> GetMaster(string id, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{_configuration.ApiServer}masters/{HttpUtility.UrlEncode(id)}");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await Request(request, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonNode?> Search(string query, string? type, CancellationToken cancellationToken)
    {
        var uri = new Uri(QueryHelpers.AddQueryString($"{_configuration.ApiServer}database/search", new Dictionary<string, string?> { { "q", query }, { "type", type } }));
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await Request(request, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> GetImage(string url, CancellationToken cancellationToken)
    {
        if (new Uri(url).Host != new Uri(_configuration.ImageServer).Host)
        {
            throw new ArgumentException($"Host does not match {_configuration.ImageServer}", nameof(url));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await Request(request, cancellationToken).ConfigureAwait(false);
        return response;
    }

    public void Dispose()
    {
        _rateLimiter.Dispose();
    }
}
