using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly DiscogsApi _api;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscogsArtistProvider"/> class.
    /// </summary>
    /// <param name="api">The Discogs API.</param>
    public DiscogsArtistProvider(DiscogsApi api)
    {
        _api = api;
    }

    /// <inheritdoc />
    public string Name => "Discogs";

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ArtistInfo searchInfo, CancellationToken cancellationToken)
    {
        var artistId = searchInfo.GetProviderId(DiscogsArtistExternalId.ProviderKey);
        if (artistId != null)
        {
            var result = await _api.GetArtist(artistId, cancellationToken).ConfigureAwait(false);
            return new[] { new RemoteSearchResult { ProviderIds = new Dictionary<string, string> { { DiscogsArtistExternalId.ProviderKey, result!["id"]!.ToString() }, }, Name = result!["name"]!.ToString(), ImageUrl = result!["images"]!.AsArray().FirstOrDefault()?["uri150"]?.ToString() } };
        }
        else
        {
            var response = await _api.Search(searchInfo.Name, "artist", cancellationToken).ConfigureAwait(false);
            return response!["results"]!.AsArray().Select(result => new RemoteSearchResult { ProviderIds = new Dictionary<string, string> { { DiscogsArtistExternalId.ProviderKey, result!["id"]!.ToString() }, }, Name = result["title"]!.ToString(), ImageUrl = result!["cover_image_url"]?.ToString(), });
        }
    }

    /// <inheritdoc />
    public async Task<MetadataResult<MusicArtist>> GetMetadata(ArtistInfo info, CancellationToken cancellationToken)
    {
        var artistId = info.GetProviderId(DiscogsArtistExternalId.ProviderKey);
        if (artistId != null)
        {
            var result = await _api.GetArtist(artistId, cancellationToken).ConfigureAwait(false);

            return new MetadataResult<MusicArtist>
            {
                Item = new MusicArtist { ProviderIds = new Dictionary<string, string> { { DiscogsArtistExternalId.ProviderKey, result!["id"]!.ToString() } }, Name = result!["name"]!.ToString(), Overview = result!["profile_html"]?.ToString() ?? result!["profile_plaintext"]?.ToString() ?? result!["profile"]?.ToString(), },
                RemoteImages = result["images"]?.AsArray()
                    .Where(image => image!["type"]!.ToString() == "primary" && image!["uri"]!.ToString().Length > 0)
                    .Select(image => (image!["uri"]!.ToString(), ImageType.Primary))
                    .ToList(),
                QueriedById = true,
                HasMetadata = true,
            };
        }

        return new MetadataResult<MusicArtist>();
    }

    /// <inheritdoc />
    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => _api.GetImage(url, cancellationToken);
}
