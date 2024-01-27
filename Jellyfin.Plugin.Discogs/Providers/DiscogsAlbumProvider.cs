using System.Collections.Generic;
using System.Globalization;
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
/// Discogs album provider.
/// </summary>
public class DiscogsAlbumProvider : IRemoteMetadataProvider<MusicAlbum, AlbumInfo>
{
    private readonly DiscogsApi _api;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscogsAlbumProvider"/> class.
    /// </summary>
    /// <param name="api">The Discogs API.</param>
    public DiscogsAlbumProvider(DiscogsApi api)
    {
        _api = api;
    }

    /// <inheritdoc />
    public string Name => "Discogs";

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(AlbumInfo searchInfo, CancellationToken cancellationToken)
    {
        var releaseId = searchInfo.GetProviderId(DiscogsReleaseExternalId.ProviderKey);
        if (releaseId != null)
        {
            var result = await _api.GetRelease(releaseId, cancellationToken).ConfigureAwait(false);
            return new[] { new RemoteSearchResult { ProviderIds = new Dictionary<string, string> { { DiscogsReleaseExternalId.ProviderKey, result!["id"]!.ToString() }, }, Name = result["title"]!.ToString(), ImageUrl = result["thumb"]!.AsArray().FirstOrDefault()?["uri150"]?.ToString() } };
        }
        else
        {
            var response = await _api.Search(searchInfo.Name, "release", cancellationToken).ConfigureAwait(false);
            return response!["results"]!.AsArray().Select(result =>
            {
                var searchResult = new RemoteSearchResult();
                searchResult.ProviderIds = new Dictionary<string, string> { { DiscogsReleaseExternalId.ProviderKey, result!["id"]!.ToString() }, };
                if (result["master_id"] != null && result["master_url"] != null)
                {
                    searchResult.ProviderIds.Add(DiscogsMasterExternalId.ProviderKey, result["master_id"]!.ToString());
                }

                searchResult.Name = result["title"]!.ToString();
                searchResult.ImageUrl = result["thumb"]?.ToString() ?? result["cover_image"]?.ToString();
                if (result["year"] != null)
                {
                    searchResult.ProductionYear = int.Parse(result["year"]!.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture);
                }

                return searchResult;
            });
        }
    }

    /// <inheritdoc />
    public async Task<MetadataResult<MusicAlbum>> GetMetadata(AlbumInfo info, CancellationToken cancellationToken)
    {
        var releaseId = info.GetProviderId(DiscogsReleaseExternalId.ProviderKey);
        if (releaseId != null)
        {
            var result = await _api.GetRelease(releaseId, cancellationToken).ConfigureAwait(false);

            return new MetadataResult<MusicAlbum>
            {
                Item = new MusicAlbum
                {
                    ProviderIds = new Dictionary<string, string> { { DiscogsReleaseExternalId.ProviderKey, result!["id"]!.ToString() } },
                    Name = result["title"]!.ToString(),
                    Overview = result["notes_html"]?.ToString() ?? result["notes_plaintext"]?.ToString() ?? result["notes"]?.ToString(),
                    Artists = result["artists"]?.AsArray().Select(artist => artist!["name"]!.ToString()).ToList(),
                    AlbumArtists = result["artists"]?.AsArray().Select(artist => artist!["name"]!.ToString()).ToList(),
                    Genres = result["genres"]?.AsArray().Select(genre => genre!.ToString()).ToArray(),
                },
                RemoteImages = result["images"]?.AsArray()
                    .Where(image => image!["type"]!.ToString() == "primary" && image["uri"]!.ToString().Length > 0)
                    .Select(image => (image!["uri"]!.ToString(), ImageType.Primary))
                    .ToList(),
                QueriedById = true,
                HasMetadata = true,
            };
        }

        return new MetadataResult<MusicAlbum>();
    }

    /// <inheritdoc />
    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => _api.GetImage(url, cancellationToken);
}
