using System.Collections.Generic;
using Jellyfin.Plugin.Discogs.ExternalIds;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.Discogs.Providers;

/// <summary>
/// External url provider for Discogs.
/// </summary>
public class DiscogsExternalUrlProvider : IExternalUrlProvider
{
    /// <inheritdoc />
    public string Name => "Discogs";

    /// <inheritdoc />
    public IEnumerable<string> GetExternalUrls(BaseItem item)
    {
        if (item.TryGetProviderId(DiscogsArtistExternalId.ProviderKey, out var externalId)
            && item is MusicArtist)
        {
            yield return $"https://www.discogs.com/artist/{externalId}";
        }

        if (item.TryGetProviderId(DiscogsMasterExternalId.ProviderKey, out externalId)
            && item is Audio or MusicAlbum)
        {
            yield return $"https://discogs.com/master/{externalId}";
        }

        if (item.TryGetProviderId(DiscogsReleaseExternalId.ProviderKey, out externalId)
            && item is Audio or MusicAlbum)
        {
            yield return $"https://discogs.com/release/{externalId}";
        }
    }
}
