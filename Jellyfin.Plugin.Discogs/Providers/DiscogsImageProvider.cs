using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Discogs.ExternalIds;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.Providers;

/// <inheritdoc />
public class DiscogsImageProvider : IRemoteImageProvider
{
    private readonly DiscogsApi _api;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscogsImageProvider"/> class.
    /// </summary>
    /// <param name="api">The Discogs API.</param>
    public DiscogsImageProvider(DiscogsApi api)
    {
        _api = api;
    }

    /// <inheritdoc />
    public string Name => Plugin.Instance!.Name;

    /// <inheritdoc />
    public bool Supports(BaseItem item) => item.HasProviderId(DiscogsArtistExternalId.ProviderKey) || item.HasProviderId(DiscogsReleaseExternalId.ProviderKey) || item.HasProviderId(DiscogsMasterExternalId.ProviderKey);

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item) => new[] { ImageType.Primary };

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        var artistId = item.GetProviderId(DiscogsArtistExternalId.ProviderKey);
        var releaseId = item.GetProviderId(DiscogsReleaseExternalId.ProviderKey);
        var masterId = item.GetProviderId(DiscogsMasterExternalId.ProviderKey);

        var images = new List<RemoteImageInfo>();

        if (artistId != null)
        {
            var result = await _api.GetArtist(artistId, cancellationToken).ConfigureAwait(false);
            if (result?["images"] != null)
            {
                foreach (var image in result["images"]!.AsArray())
                {
                    if (image!["uri"]!.ToString().Length > 0)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = image["uri"]!.ToString(),
                            ProviderName = Name,
                            Type = ImageType.Primary,
                            ThumbnailUrl = image["uri150"]!.ToString(),
                            Width = image["width"]?.Deserialize<int>(),
                            Height = image["height"]?.Deserialize<int>()
                        });
                    }
                }
            }
        }

        if (releaseId != null)
        {
            var result = await _api.GetRelease(releaseId, cancellationToken).ConfigureAwait(false);
            if (result?["images"] != null)
            {
                foreach (var image in result["images"]!.AsArray())
                {
                    if (image!["uri"]!.ToString().Length > 0)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = image["uri"]!.ToString(),
                            ProviderName = Name,
                            Type = ImageType.Primary,
                            ThumbnailUrl = image["uri150"]!.ToString(),
                            Width = image["width"]?.Deserialize<int>(),
                            Height = image["height"]?.Deserialize<int>()
                        });
                    }
                }
            }
        }

        if (masterId != null)
        {
            var result = await _api.GetMaster(masterId, cancellationToken).ConfigureAwait(false);
            if (result?["images"] != null)
            {
                foreach (var image in result["images"]!.AsArray())
                {
                    if (image!["uri"]!.ToString().Length > 0)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = image["uri"]!.ToString(),
                            ProviderName = Name,
                            Type = ImageType.Primary,
                            ThumbnailUrl = image["uri150"]!.ToString(),
                            Width = image["width"]?.Deserialize<int>(),
                            Height = image["height"]?.Deserialize<int>()
                        });
                    }
                }
            }
        }

        return images;
    }

    /// <inheritdoc />
    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) => _api.GetImage(url, cancellationToken);
}
