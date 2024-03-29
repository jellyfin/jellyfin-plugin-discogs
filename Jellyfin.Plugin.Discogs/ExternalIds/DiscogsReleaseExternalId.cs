﻿using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.ExternalIds;

/// <summary>
/// Discogs release external id.
/// </summary>
public class DiscogsReleaseExternalId : IExternalId
{
    /// <summary>
    /// The key.
    /// </summary>
    public const string ProviderKey = "DiscogsRelease";

    /// <inheritdoc />
    public string ProviderName => "Discogs";

    /// <inheritdoc />
    public string Key => ProviderKey;

    /// <inheritdoc />
    public ExternalIdMediaType? Type => ExternalIdMediaType.ReleaseGroup;

    /// <inheritdoc />
    public string UrlFormatString => "https://www.discogs.com/release/{0}";

    /// <inheritdoc />
    public bool Supports(IHasProviderIds item) => item is Audio || item is MusicAlbum;
}
