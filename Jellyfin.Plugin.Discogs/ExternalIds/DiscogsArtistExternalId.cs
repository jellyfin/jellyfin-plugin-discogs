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
    /// <summary>
    /// The key.
    /// </summary>
    public const string ProviderKey = "DiscogsArtist";

    /// <inheritdoc />
    public string ProviderName => "Discogs";

    /// <inheritdoc />
    public string Key => ProviderKey;

    /// <inheritdoc />
    public ExternalIdMediaType? Type => ExternalIdMediaType.Artist;

    /// <inheritdoc />
    public bool Supports(IHasProviderIds item) => item is MusicArtist;
}
