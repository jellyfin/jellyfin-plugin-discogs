using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiscogsApiClient;
using DiscogsApiClient.Authentication;
using DiscogsApiClient.QueryParameters;
using Jellyfin.Extensions;
using Jellyfin.Plugin.Discogs.Configuration;
using Jellyfin.Plugin.Discogs.ExternalIds;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.Providers;

/// <summary>
/// Discogs artist provider.
/// </summary>
public class DiscogsArtistProvider : IRemoteMetadataProvider<MusicArtist, ArtistInfo>
{
    private readonly IDiscogsApiClient _discogsApiClient;
    private readonly IDiscogsAuthenticationService _discogsAuthenticationService;
    private readonly PluginConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscogsArtistProvider"/> class.
    /// </summary>
    /// <param name="discogsApiClient">The discogsApiClient.</param>
    /// <param name="discogsAuthenticationService">The discogsAuthenticationService.</param>
    /// <param name="configuration">The configuration.</param>
    public DiscogsArtistProvider(IDiscogsApiClient discogsApiClient, IDiscogsAuthenticationService discogsAuthenticationService, PluginConfiguration configuration)
    {
        _discogsApiClient = discogsApiClient;
        _discogsAuthenticationService = discogsAuthenticationService;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public string Name => "Discogs";

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ArtistInfo searchInfo, CancellationToken cancellationToken)
    {
        _discogsAuthenticationService.AuthenticateWithPersonalAccessToken(_configuration.ApiToken);

        var artistId = searchInfo.GetProviderId(DiscogsArtistExternalId.ProviderKey);
        if (artistId != null && int.TryParse(artistId, out var artistIdInt))
        {
            var result = await _discogsApiClient.GetArtist(artistIdInt, cancellationToken).ConfigureAwait(false);
            return new[] { new RemoteSearchResult { ProviderIds = new Dictionary<string, string> { { DiscogsArtistExternalId.ProviderKey, result.Id.ToString(CultureInfo.InvariantCulture) }, }, Name = result.Name, ImageUrl = result.Images.FirstOrDefault()?.ImageUri150 } };
        }
        else
        {
            var response = await _discogsApiClient.SearchDatabase(new SearchQueryParameters { Query = searchInfo.Name, Type = "artist", }, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Results.Select(result => new RemoteSearchResult { ProviderIds = new Dictionary<string, string> { { DiscogsArtistExternalId.ProviderKey, result.Id.ToString(CultureInfo.InvariantCulture) }, }, Name = result.Title, ImageUrl = result.CoverImageUrl, });
        }
    }

    /// <inheritdoc />
    public async Task<MetadataResult<MusicArtist>> GetMetadata(ArtistInfo info, CancellationToken cancellationToken)
    {
        var artistId = info.GetProviderId(DiscogsArtistExternalId.ProviderKey);
        if (artistId != null && int.TryParse(artistId, out var artistIdInt))
        {
            _discogsAuthenticationService.AuthenticateWithPersonalAccessToken(_configuration.ApiToken);
            var result = await _discogsApiClient.GetArtist(artistIdInt, cancellationToken).ConfigureAwait(false);

            return new MetadataResult<MusicArtist>
            {
                Item = new MusicArtist { ProviderIds = new Dictionary<string, string>() { { DiscogsArtistExternalId.ProviderKey, result.Id.ToString(CultureInfo.InvariantCulture) }, }, Name = result.Name, Overview = result.Profile, },
                RemoteImages = result.Images
                    .Where(image => image.Type == DiscogsApiClient.Contract.ImageType.Primary)
                    .Select(image => (image.ImageUri, ImageType.Primary))
                    .ToList(),
                QueriedById = true,
                HasMetadata = true,
            };
        }

        return new MetadataResult<MusicArtist>();
    }

    /// <inheritdoc />
    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
