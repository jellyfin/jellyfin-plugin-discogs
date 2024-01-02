using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.ExternalIds;

/// <summary>
/// Discogs artist external id.
/// </summary>
public class DiscogsArtistExternalId : IExternalId
{
    /// <inheritdoc />
    public string ProviderName => "Discogs";

    /// <inheritdoc />
    public string Key => "DiscogsArtist";

    /// <inheritdoc />
    public ExternalIdMediaType? Type => ExternalIdMediaType.Artist;

    /// <inheritdoc />
    public string UrlFormatString => "https://www.discogs.com/artist/{0}";

    /// <inheritdoc />
    public bool Supports(IHasProviderIds item) => item is MusicArtist;
}
