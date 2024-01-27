using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.ExternalIds;

/// <summary>
/// Discogs master external id.
/// </summary>
public class DiscogsMasterExternalId : IExternalId
{
    /// <summary>
    /// The key.
    /// </summary>
    public const string ProviderKey = "DiscogsMaster";

    /// <inheritdoc />
    public string ProviderName => "Discogs";

    /// <inheritdoc />
    public string Key => ProviderKey;

    /// <inheritdoc />
    public ExternalIdMediaType? Type => ExternalIdMediaType.Album;

    /// <inheritdoc />
    public string UrlFormatString => "https://www.discogs.com/master/{0}";

    /// <inheritdoc />
    public bool Supports(IHasProviderIds item) => item is Audio || item is MusicAlbum;
}
